
using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomGameRating
{
    // 게임 하나의 평가 정보
    public class GameReview
    {
        public string GameName { get; set; }

        // 평가 항목 ID별 점수
        public Dictionary<string, int> Scores { get; set; }

        public string Review { get; set; }

        public DateTime? LastEvaluatedAt { get; set; }

        public GameReview()
        {
            GameName = "";
            Review = "";
            Scores = new Dictionary<string, int>();
        }

        // 활성화된 평가 항목들의 평균 점수
        public int GetAverageScore(IEnumerable<string> activeCategoryIds)
        {
            if (Scores == null || activeCategoryIds == null)
            {
                return 0;
            }

            // 중복 ID는 한 번만 계산
            var uniqueIds = activeCategoryIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.Ordinal)
                .ToList();

            // 실제 점수가 저장된 항목만 평균에 포함
            var scores = uniqueIds
                .Where(id => Scores.ContainsKey(id))
                .Select(id => Scores[id])
                .ToList();

            if (scores.Count == 0)
            {
                return 0;
            }

            return (int)Math.Round(
                scores.Average(),
                MidpointRounding.AwayFromZero
            );
        }
    }

    // JSON 파일의 최상위 데이터
    public class ReviewDatabase
    {
        public int SchemaVersion { get; set; }

        public Dictionary<string, GameReview> Games { get; set; }

        public ReviewDatabase()
        {
            SchemaVersion = 2;
            Games = new Dictionary<string, GameReview>();
        }
    }

    // 사용했던 평가 ID 목록 캐시
    public class CategoryIdCache
    {
        public List<string> Ids { get; set; }

        public CategoryIdCache()
        {
            Ids = new List<string>();
        }
    }
}
