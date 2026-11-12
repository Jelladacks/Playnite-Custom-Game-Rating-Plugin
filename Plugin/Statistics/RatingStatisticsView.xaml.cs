
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

namespace CustomGameRating
{
    public partial class RatingStatisticsView : UserControl
    {
        // 최근 평가한 게임 카드: 기본 개수 / 공간 부족 시 개수
        private const int RecentGameMaxColumns = 10;
        private const int RecentGameMinColumns = 5;

        // 최근 평가 카드 아이콘의 최소 크기(px). 이보다 작아지면 5개로 줄임
        private const double RecentGameMinIconSize = 64;

        // 최근 평가 카드의 Margin(3) + BorderThickness(2) + Padding(6), 좌우 합계
        private const double RecentGameCardHorizontalChrome = (3 + 2 + 6) * 2;

        // 대시보드가 기본 글자 크기로 보기 좋은 너비.
        // 대시보드가 이보다 넓어지면 왼쪽 요약 글자도 같은 비율로 커짐
        private const double SummaryBaseWidth = 1150;
        private const double SummaryMaxFontScale = 1.7;

        // 대시보드 카드 비율 (세로 / 가로 = 9 / 6)
        private const double CardHeightRatio = 1.5;

        // 대시보드 카드 Margin(4), 상하/좌우 합계
        private const double DashboardCardMargin = 4 * 2;

        // 요약 영역과 카드 영역 사이 간격 (DashboardCardHost의 왼쪽 Margin)
        private const double DashboardCardAreaMargin = 12;

        // 최근 평가 게임 카드가 이보다 좁아지면 숨김
        private const double LatestCardMinWidth = 80;

        private static readonly Dictionary<string, double> DefaultFontSizes =
            new Dictionary<string, double>
            {
                { "FontSize", 14 },
                { "FontSizeLarge", 15 },
                { "FontSizeLarger", 20 },
                { "FontSizeLargest", 29 }
            };

        public static readonly DependencyProperty RecentGameColumnsProperty =
            DependencyProperty.Register(
                nameof(RecentGameColumns),
                typeof(int),
                typeof(RatingStatisticsView),
                new PropertyMetadata(RecentGameMaxColumns));

        public int RecentGameColumns
        {
            get => (int)GetValue(RecentGameColumnsProperty);
            set => SetValue(RecentGameColumnsProperty, value);
        }

        private readonly CustomGameRating plugin;

        private List<RatingStatisticsGame> allGames =
            new List<RatingStatisticsGame>();

        private List<RatingStatisticsGame> recentGames =
            new List<RatingStatisticsGame>();

        private Dictionary<string, double> baseFontSizes;
        private double currentFontScale = 1;

        public RatingStatisticsView(CustomGameRating plugin)
        {
            InitializeComponent();

            this.plugin = plugin;

            Loaded += (s, e) =>
            {
                RefreshStatistics();
            };

            plugin.ReviewSaved += OnReviewSaved;
        }

        private void OnReviewSaved(Guid gameId)
        {
            RefreshStatistics();
        }

        private void RefreshStatistics()
        {
            var calculator = new RatingStatisticsCalculator(plugin);
            var data = calculator.Calculate();

            // 전체 요약
            TotalGamesText.Text = data.TotalGames.ToString();
            RatedGamesText.Text = data.RatedGames.ToString();
            UnratedGamesText.Text = data.UnratedGames.ToString();
            OverallAverageText.Text = data.OverallAverage.ToString();

            // 평가 진행률
            double percentage = data.TotalGames > 0
                ? data.RatedGames * 100.0 / data.TotalGames
                : 0;

            RatedPercentageText.Text = $"{percentage:0.#}%";
            RatedProgressBar.Value = percentage;

            // 최고 평가 게임
            if (data.HighestRatedGame != null)
            {
                HighestGameText.Text = data.HighestRatedGame.GameName;
                HighestScoreText.Text =
                    $"★{data.HighestRatedGame.AverageScore}";

                HighestGameImage.ImageSource =
                    data.HighestRatedGame.CoverImageSource;
            }
            else
            {
                HighestGameText.Text = "-";
                HighestScoreText.Text = Loc.Get("LOC_CGR_NoRatingData");
                HighestGameImage.ImageSource = null;
            }

            // 최근 평가 게임 (대시보드 카드)
            var latestGame = data.RecentGames?.FirstOrDefault();

            if (latestGame != null)
            {
                LatestGameText.Text = latestGame.GameName;
                LatestScoreText.Text = $"★{latestGame.AverageScore}";
                LatestDateText.Text = latestGame.LastEvaluatedAt.HasValue
                    ? Loc.Format("LOC_CGR_Stats_RatedOn", latestGame.LastEvaluatedAt.Value)
                    : "";

                LatestGameImage.ImageSource = latestGame.CoverImageSource;
            }
            else
            {
                LatestGameText.Text = "-";
                LatestScoreText.Text = Loc.Get("LOC_CGR_NoRatingData");
                LatestDateText.Text = "";
                LatestGameImage.ImageSource = null;
            }

            // 최저 평가 게임 (작은 카드)
            if (data.LowestRatedGame != null)
            {
                LowestGameText.Text = data.LowestRatedGame.GameName;
                LowestScoreText.Text =
                    $"★{data.LowestRatedGame.AverageScore}";

                LowestGameImage.ImageSource =
                    data.LowestRatedGame.CoverImageSource;
            }
            else
            {
                LowestGameText.Text = "-";
                LowestScoreText.Text = Loc.Get("LOC_CGR_NoRatingData");
                LowestGameImage.ImageSource = null;
            }

            // 후한 평가 요소
            if (data.MostGenerousCategory != null)
            {
                MostGenerousCategoryText.Text =
                    data.MostGenerousCategory.DisplayName;

                MostGenerousScoreText.Text =
                    $"★{data.MostGenerousCategory.AverageScore}";
            }
            else
            {
                MostGenerousCategoryText.Text = "-";
                MostGenerousScoreText.Text = Loc.Get("LOC_CGR_NoRatingData");
            }

            // 깐깐한 평가 요소
            if (data.StrictestCategory != null)
            {
                StrictestCategoryText.Text =
                    data.StrictestCategory.DisplayName;

                StrictestScoreText.Text =
                    $"★{data.StrictestCategory.AverageScore}";
            }
            else
            {
                StrictestCategoryText.Text = "-";
                StrictestScoreText.Text = Loc.Get("LOC_CGR_NoRatingData");
            }

            // 선호하는 게임
            PreferredGameList.ItemsSource = data.PreferredGames;

            // 최근 평가한 게임 카드
            recentGames = data.RecentGames ?? new List<RatingStatisticsGame>();
            UpdateRecentGameCards();

            // 카테고리별 평균
            CategoryList.ItemsSource = data.Categories;

            // 점수 분포
            ScoreRangeList.ItemsSource = data.ScoreRanges;

            // 전체 게임 목록
            allGames = data.AllGames ?? new List<RatingStatisticsGame>();

            ApplyGameListFilters();
        }

        // 대시보드 너비에 맞춰 왼쪽 요약 영역 글자 크기 조정
        private void DashboardGrid_SizeChanged(
            object sender,
            SizeChangedEventArgs e)
        {
            if (e.WidthChanged)
            {
                double scale = e.NewSize.Width / SummaryBaseWidth;
                scale = Math.Max(1, Math.Min(SummaryMaxFontScale, scale));

                if (Math.Abs(scale - currentFontScale) >= 0.01)
                    ApplySummaryFontScale(scale);
            }

            UpdateDashboardCardSizes();
        }

        // 요약 영역 높이가 바뀌면 (글자 크기 변경 등) 카드 크기 다시 계산
        private void SummaryPanel_SizeChanged(
            object sender,
            SizeChangedEventArgs e)
        {
            if (e.HeightChanged)
                UpdateDashboardCardSizes();
        }

        private void UpdateDashboardCardSizes()
        {
            double summaryHeight = SummaryPanel.ActualHeight;
            double dashboardWidth = DashboardGrid.ActualWidth;

            if (summaryHeight <= 0 || dashboardWidth <= 0)
                return;

            double highestHeight = summaryHeight - DashboardCardMargin;
            double maxCardWidth = highestHeight / CardHeightRatio;

            // 오른쪽 영역 너비 결정
            // - 기본: 대시보드의 3/5
            // - 카드 두 개가 모두 최대 크기(최고 평가 카드와 같은 크기)로 들어가고도
            //   공간이 남으면, 오른쪽은 필요한 만큼만 쓰고 남는 공간은 왼쪽 요약 영역에 줌
            double defaultAreaWidth =
                dashboardWidth * 3 / 5 - DashboardCardAreaMargin;

            double neededAreaWidth = (maxCardWidth + DashboardCardMargin) * 2;

            double areaWidth;

            if (neededAreaWidth < defaultAreaWidth)
            {
                areaWidth = neededAreaWidth;
                SetDashboardColumns(
                    new GridLength(1, GridUnitType.Star),
                    new GridLength(areaWidth + DashboardCardAreaMargin));
            }
            else
            {
                areaWidth = defaultAreaWidth;
                SetDashboardColumns(
                    new GridLength(2, GridUnitType.Star),
                    new GridLength(3, GridUnitType.Star));
            }

            if (areaWidth <= 0)
                return;

            // 카드 영역은 Canvas 안에 있으므로 크기를 직접 지정
            DashboardCardArea.Width = areaWidth;
            DashboardCardArea.Height = summaryHeight;

            // 최고 평가 게임: 세로는 요약 영역과 같게, 가로는 6:9 비율
            // (공간이 모자라면 오른쪽 영역 너비까지만)
            double highestWidth = Math.Min(
                highestHeight / CardHeightRatio,
                areaWidth - DashboardCardMargin);

            HighestGameCard.Width = Math.Max(0, highestWidth);

            // 최근 평가 게임: 남은 가로 공간에 6:9 비율로,
            // 세로는 최고 평가 게임보다 커지지 않게
            double latestWidth =
                areaWidth - (highestWidth + DashboardCardMargin)
                - DashboardCardMargin;

            double latestHeight = latestWidth * CardHeightRatio;

            if (latestHeight > highestHeight)
            {
                latestHeight = highestHeight;
                latestWidth = latestHeight / CardHeightRatio;
            }

            if (latestWidth < LatestCardMinWidth)
            {
                LatestGameCard.Visibility = Visibility.Collapsed;
                return;
            }

            LatestGameCard.Visibility = Visibility.Visible;
            LatestGameCard.Width = latestWidth;
            LatestGameCard.Height = latestHeight;
        }

        private void SetDashboardColumns(GridLength summary, GridLength cards)
        {
            // 같은 값이면 다시 넣지 않음 (불필요한 레이아웃 재계산 방지)
            if (!SummaryColumn.Width.Equals(summary))
                SummaryColumn.Width = summary;

            if (!CardColumn.Width.Equals(cards))
                CardColumn.Width = cards;
        }

        private void ApplySummaryFontScale(double scale)
        {
            if (baseFontSizes == null)
            {
                baseFontSizes = new Dictionary<string, double>();

                foreach (var pair in DefaultFontSizes)
                {
                    baseFontSizes[pair.Key] =
                        TryFindResource(pair.Key) is double size
                            ? size
                            : pair.Value;
                }
            }

            currentFontScale = scale;

            // DynamicResource는 가장 가까운 리소스를 찾으므로
            // 요약 영역에만 크기를 덮어쓰면 그 안의 글자만 커짐
            foreach (var pair in baseFontSizes)
            {
                SummaryPanel.Resources[pair.Key] = pair.Value * scale;
            }

            // FontSize를 지정하지 않은 일반 텍스트
            TextElement.SetFontSize(
                SummaryPanel,
                baseFontSizes["FontSize"] * scale);

            // 최저 평가 게임 카드는 크기 조정에서 제외 → 기본 크기로 되돌림
            foreach (var pair in baseFontSizes)
            {
                LowestGameCard.Resources[pair.Key] = pair.Value;
            }

            TextElement.SetFontSize(
                LowestGameCard,
                baseFontSizes["FontSize"]);
        }

        // 화면 너비에 따라 최근 평가 카드를 10개 또는 5개로 배치
        private void RecentGameCards_SizeChanged(
            object sender,
            SizeChangedEventArgs e)
        {
            if (!e.WidthChanged)
                return;

            double iconSize =
                e.NewSize.Width / RecentGameMaxColumns
                - RecentGameCardHorizontalChrome;

            int columns = iconSize >= RecentGameMinIconSize
                ? RecentGameMaxColumns
                : RecentGameMinColumns;

            if (columns == RecentGameColumns)
                return;

            RecentGameColumns = columns;
            UpdateRecentGameCards();
        }

        private void UpdateRecentGameCards()
        {
            RecentGameCards.ItemsSource = recentGames
                .Take(RecentGameColumns)
                .ToList();
        }

        private void GameSearchBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            ApplyGameListFilters();
        }

        private void Filter_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            ApplyGameListFilters();
        }

        private void ApplyGameListFilters()
        {
            // XAML 초기화 중 발생하는 이벤트 방지
            if (GameList == null)
                return;

            IEnumerable<RatingStatisticsGame> filteredGames = allGames;

            // 1. 이름 검색
            string searchText = GameSearchBox.Text?.Trim();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filteredGames = filteredGames.Where(game =>
                    !string.IsNullOrEmpty(game.GameName) &&
                    game.GameName.IndexOf(
                        searchText,
                        StringComparison.OrdinalIgnoreCase) >= 0);
            }

            // 2. 평가 여부 필터
            int ratingFilter = RatingFilterComboBox.SelectedIndex;

            if (ratingFilter == 1)
            {
                filteredGames = filteredGames.Where(game => game.IsRated);
            }
            else if (ratingFilter == 2)
            {
                filteredGames = filteredGames.Where(game => !game.IsRated);
            }

            // 3. 점수 구간 필터
            int scoreFilter = ScoreFilterComboBox.SelectedIndex;

            switch (scoreFilter)
            {
                case 1:
                    filteredGames = filteredGames.Where(game =>
                        game.IsRated &&
                        game.AverageScore >= 0 &&
                        game.AverageScore <= 59);
                    break;

                case 2:
                    filteredGames = filteredGames.Where(game =>
                        game.IsRated &&
                        game.AverageScore >= 60 &&
                        game.AverageScore <= 79);
                    break;

                case 3:
                    filteredGames = filteredGames.Where(game =>
                        game.IsRated &&
                        game.AverageScore >= 80 &&
                        game.AverageScore <= 89);
                    break;

                case 4:
                    filteredGames = filteredGames.Where(game =>
                        game.IsRated &&
                        game.AverageScore >= 90 &&
                        game.AverageScore <= 100);
                    break;
            }

            // 4. 정렬
            int sortOption = SortComboBox.SelectedIndex;

            switch (sortOption)
            {
                case 0:
                    filteredGames = filteredGames
                        .OrderByDescending(game => game.IsRated)
                        .ThenByDescending(game => game.AverageScore);
                    break;

                case 1:
                    filteredGames = filteredGames
                        .Where(game => game.IsRated)
                        .OrderBy(game => game.AverageScore);
                    break;

                case 2:
                    filteredGames = filteredGames
                        .OrderByDescending(game =>
                            game.LastEvaluatedAt ?? DateTime.MinValue);
                    break;

                case 3:
                    filteredGames = filteredGames
                        .OrderBy(game => game.GameName);
                    break;
            }

            GameList.ItemsSource = filteredGames.ToList();
        }

        // 전체 평가 목록 더블클릭
        private void GameList_MouseDoubleClick(
            object sender,
            MouseButtonEventArgs e)
        {
            var item = ItemsControl.ContainerFromElement(
                GameList,
                e.OriginalSource as DependencyObject) as ListViewItem;

            if (item == null)
                return;

            if (!(item.DataContext is RatingStatisticsGame selectedGame))
                return;

            OpenSelectedGame(selectedGame);
        }

        // 최근 평가 카드 더블클릭
        private void RecentGameCard_MouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            if (e.ClickCount != 2)
                return;

            var border = sender as Border;

            if (border == null)
                return;

            if (!(border.DataContext is RatingStatisticsGame selectedGame))
                return;

            OpenSelectedGame(selectedGame);
        }

        // 선택된 게임의 기존 평가 창 열기
        private void OpenSelectedGame(RatingStatisticsGame selectedGame)
        {
            if (selectedGame == null)
                return;

            var game = plugin.PlayniteApi.Database.Games
                .FirstOrDefault(g => g.Id == selectedGame.GameId);

            if (game == null)
                return;

            plugin.OpenReview(game);
        }
    }

    // 값 + ConverterParameter (예: -24 → 좌우 여백 24px 만큼 뺀 너비)
    public class OffsetConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            if (!(value is double size) || double.IsNaN(size))
                return double.NaN;

            double offset = 0;

            if (parameter != null)
            {
                double.TryParse(
                    parameter.ToString(),
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out offset);
            }

            double result = size + offset;

            return result > 0 ? result : double.NaN;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    // 요소의 가로 길이 × 비율(ConverterParameter) = 세로 길이
    // 예) 1.5 → 가로 6 : 세로 9, 1 → 정사각형
    public class AspectRatioConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            if (!(value is double width) || width <= 0 || double.IsNaN(width))
                return double.NaN;

            double ratio = ParseRatio(parameter?.ToString());

            return width * ratio;
        }

        // "1.5" 같은 숫자 또는 "2/9" 같은 분수 형태 지원
        private static double ParseRatio(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return 1;

            string[] parts = text.Split('/');

            if (parts.Length == 2 &&
                TryParseNumber(parts[0], out double numerator) &&
                TryParseNumber(parts[1], out double denominator) &&
                denominator != 0)
            {
                return numerator / denominator;
            }

            return TryParseNumber(text, out double ratio) ? ratio : 1;
        }

        private static bool TryParseNumber(string text, out double value)
        {
            return double.TryParse(
                text.Trim(),
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out value);
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
