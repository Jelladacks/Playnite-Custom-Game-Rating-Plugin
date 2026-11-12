
using System;
using System.Collections.Generic;
using System.Linq;
using Playnite.SDK.Models;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CustomGameRating
{
    public class RatingStatisticsCalculator
    {
        private const int PreferredPoolSize = 15;
        private const int PreferredDisplayCount = 5;

        private static readonly Random random = new Random();

        private readonly CustomGameRating plugin;
        private readonly ReviewStore store;

        public RatingStatisticsCalculator(CustomGameRating plugin)
        {
            this.plugin = plugin;
            store = new ReviewStore(plugin);
        }

        public RatingStatisticsData Calculate()
        {
            var database = store.LoadDatabase();
            var games = plugin.PlayniteApi.Database.Games.ToList();

            // 활성화된 카테고리 가져오기
            var activeCategories = plugin.CurrentSettings.Categories
                .Where(category =>
                    category.Enabled &&
                    !string.IsNullOrWhiteSpace(category.Id))
                .OrderBy(category => category.Order)
                .GroupBy(category => category.Id, StringComparer.Ordinal)
                .Select(group => group.First())
                .ToList();

            var activeIds = activeCategories
                .Select(category => category.Id)
                .ToList();

            var ratedGames = new List<RatingStatisticsGame>();
            var categoryScores = new Dictionary<string, List<int>>();
            var allGames = new List<RatingStatisticsGame>();

            foreach (var id in activeIds)
            {
                categoryScores[id] = new List<int>();
            }

            int unratedCount = 0;

            foreach (var game in games)
            {
                string key = game.Id.ToString();

                if (!database.Games.TryGetValue(key, out GameReview review)
                    || review == null
                    || review.Scores == null)
                {
                    unratedCount++;

                    allGames.Add(new RatingStatisticsGame
                    {
                        GameId = game.Id,
                        GameName = game.Name,
                        CoverImage = game.CoverImage,
                        Icon = game.Icon,
                        AverageScore = 0,
                        IsRated = false,

                        CoverImageSource = LoadGameImage(game.CoverImage),
                        IconSource = LoadGameImage(game.Icon)
                    });

                    continue;
                }

                var gameScores = activeIds
                    .Where(id => review.Scores.ContainsKey(id))
                    .Select(id => review.Scores[id])
                    .ToList();

                if (gameScores.Count == 0)
                {
                    unratedCount++;

                    allGames.Add(new RatingStatisticsGame
                    {
                        GameId = game.Id,
                        GameName = game.Name,
                        CoverImage = game.CoverImage,
                        Icon = game.Icon,
                        AverageScore = 0,
                        IsRated = false,

                        CoverImageSource = LoadGameImage(game.CoverImage),
                        IconSource = LoadGameImage(game.Icon)
                    });

                    continue;
                }

                int averageScore = review.GetAverageScore(activeIds);

                var statisticsGame = new RatingStatisticsGame
                {
                    GameId = game.Id,
                    GameName = game.Name,
                    CoverImage = game.CoverImage,
                    Icon = game.Icon,
                    AverageScore = averageScore,
                    LastEvaluatedAt = review.LastEvaluatedAt,
                    IsRated = true,

                    CoverImageSource = LoadGameImage(game.CoverImage),
                    IconSource = LoadGameImage(game.Icon)
                };

                ratedGames.Add(statisticsGame);
                allGames.Add(statisticsGame);

                foreach (var id in activeIds)
                {
                    if (review.Scores.TryGetValue(id, out int score))
                        categoryScores[id].Add(score);
                }
            }

            // 카테고리별 평균
            var categories = activeCategories
                .Select(category =>
                {
                    var scores = categoryScores[category.Id];

                    return new RatingStatisticsCategory
                    {
                        Id = category.Id,
                        DisplayName = category.DisplayName,
                        AverageScore = scores.Count > 0
                            ? (int)Math.Round(
                                scores.Average(),
                                MidpointRounding.AwayFromZero)
                            : 0,
                        RatedCount = scores.Count
                    };
                })
                .ToList();

            // 점수 내림차순 정렬
            var sortedGames = ratedGames
                .OrderByDescending(game => game.AverageScore)
                .ThenBy(game => game.GameName)
                .ToList();

            // 선호하는 게임: 상위 15개 중 무작위 5개 (보여줄 때는 점수 높은 순)
            var preferredGames = sortedGames
                .Take(PreferredPoolSize)
                .OrderBy(game => random.Next())
                .Take(PreferredDisplayCount)
                .OrderByDescending(game => game.AverageScore)
                .ThenBy(game => game.GameName)
                .ToList();

            // 평가 항목 중 평균이 가장 높은 항목
            var mostGenerousCategory = categories
                .Where(category => category.RatedCount > 0)
                .OrderByDescending(category => category.AverageScore)
                .ThenBy(category => category.DisplayName)
                .FirstOrDefault();

            // 평가 항목 중 평균이 가장 낮은 항목
            var strictestCategory = categories
                .Where(category => category.RatedCount > 0)
                .OrderBy(category => category.AverageScore)
                .ThenBy(category => category.DisplayName)
                .FirstOrDefault();

            // 최근 평가한 게임 10개
            var recentGames = ratedGames
                .Where(game => game.LastEvaluatedAt.HasValue)
                .OrderByDescending(game => game.LastEvaluatedAt.Value)
                .Take(10)
                .ToList();

            // 점수 분포
            var scoreRanges = new List<RatingScoreRange>
            {
                CreateRange("0~19", ratedGames, 0, 19),
                CreateRange("20~39", ratedGames, 20, 39),
                CreateRange("40~59", ratedGames, 40, 59),
                CreateRange("60~79", ratedGames, 60, 79),
                CreateRange("80~89", ratedGames, 80, 89),
                CreateRange("90~100", ratedGames, 90, 100)
            };

            // 전체 평가 게임 중 각 구간이 차지하는 비율
            foreach (var range in scoreRanges)
            {
                range.Percentage = ratedGames.Count > 0
                    ? range.Count * 100.0 / ratedGames.Count
                    : 0;
            }

            return new RatingStatisticsData
            {
                TotalGames = games.Count,
                RatedGames = ratedGames.Count,
                UnratedGames = unratedCount,

                OverallAverage = ratedGames.Count > 0
                    ? (int)Math.Round(
                        ratedGames.Average(game => game.AverageScore),
                        MidpointRounding.AwayFromZero)
                    : 0,

                Games = sortedGames,
                Categories = categories,
                RecentGames = recentGames,
                ScoreRanges = scoreRanges,

                AllGames = allGames,
                PreferredGames = preferredGames,
                MostGenerousCategory = mostGenerousCategory,
                StrictestCategory = strictestCategory,

                HighestRatedGame = sortedGames.FirstOrDefault(),
                LowestRatedGame = sortedGames.LastOrDefault()
            };
        }

        private RatingScoreRange CreateRange(
            string label,
            List<RatingStatisticsGame> games,
            int min,
            int max)
        {
            return new RatingScoreRange
            {
                Label = label,
                Count = games.Count(game =>
                    game.AverageScore >= min &&
                    game.AverageScore <= max)
            };
        }

        private ImageSource LoadGameImage(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
                return null;

            try
            {
                string fullPath;

                // HTTP/HTTPS 이미지 주소도 처리
                if (Uri.TryCreate(imagePath, UriKind.Absolute, out Uri imageUri) &&
                    (imageUri.Scheme == Uri.UriSchemeHttp ||
                     imageUri.Scheme == Uri.UriSchemeHttps))
                {
                    var webImage = new BitmapImage();

                    webImage.BeginInit();
                    webImage.CacheOption = BitmapCacheOption.OnLoad;
                    webImage.UriSource = imageUri;
                    webImage.EndInit();
                    webImage.Freeze();

                    return webImage;
                }

                // file:// URI 처리
                if (Uri.TryCreate(imagePath, UriKind.Absolute, out Uri fileUri) &&
                    fileUri.IsFile)
                {
                    fullPath = fileUri.LocalPath;
                }
                // 이미 절대 경로인 경우
                else if (Path.IsPathRooted(imagePath))
                {
                    fullPath = imagePath;
                }
                // Playnite 데이터베이스의 이미지 파일 ID인 경우
                else
                {
                    fullPath = plugin.PlayniteApi.Database.GetFullFilePath(imagePath);
                }

                if (string.IsNullOrWhiteSpace(fullPath) || !File.Exists(fullPath))
                    return null;

                var image = new BitmapImage();

                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = new Uri(fullPath, UriKind.Absolute);
                image.EndInit();
                image.Freeze();

                return image;
            }
            catch
            {
                // 이미지 ID가 없거나 파일을 읽을 수 없는 경우
                return null;
            }
        }
    }

    public class RatingStatisticsData
    {
        public int TotalGames { get; set; }
        public int RatedGames { get; set; }
        public int UnratedGames { get; set; }
        public int OverallAverage { get; set; }

        public List<RatingStatisticsGame> Games { get; set; }
        public List<RatingStatisticsCategory> Categories { get; set; }
        public List<RatingStatisticsGame> RecentGames { get; set; }
        public List<RatingScoreRange> ScoreRanges { get; set; }

        public RatingStatisticsGame HighestRatedGame { get; set; }
        public RatingStatisticsGame LowestRatedGame { get; set; }

        public List<RatingStatisticsGame> AllGames { get; set; }
        public List<RatingStatisticsGame> PreferredGames { get; set; }

        public RatingStatisticsCategory MostGenerousCategory { get; set; }
        public RatingStatisticsCategory StrictestCategory { get; set; }
    }

    public class RatingStatisticsGame
    {
        public Guid GameId { get; set; }

        public string GameName { get; set; }

        public string CoverImage { get; set; }

        public string Icon { get; set; }
        public ImageSource CoverImageSource { get; set; }
        public ImageSource IconSource { get; set; }

        public int AverageScore { get; set; }

        public int Rank { get; set; }

        public DateTime? LastEvaluatedAt { get; set; }

        public bool IsRated { get; set; }
    }

    public class RatingStatisticsCategory
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public int AverageScore { get; set; }
        public int RatedCount { get; set; }
    }

    public class RatingScoreRange
    {
        public string Label { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
    }
}
