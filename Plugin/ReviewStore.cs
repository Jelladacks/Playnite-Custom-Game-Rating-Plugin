using Playnite.SDK.Data;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CustomGameRating
{
    public class ReviewStore
    {
        private readonly string filePath;
        private readonly string categoryCachePath;

        public ReviewStore(CustomGameRating plugin)
        {
            string dataPath = plugin.GetPluginUserDataPath();

            filePath = Path.Combine(dataPath, "customRating.json");
            categoryCachePath = Path.Combine(dataPath, "categoryIds.json");
        }

        // 전체 평가 데이터 불러오기
        public ReviewDatabase LoadDatabase()
        {
            if (!File.Exists(filePath))
            {
                return new ReviewDatabase();
            }

            var database =
                Serialization.FromJsonFile<ReviewDatabase>(filePath);

            if (database == null)
            {
                return new ReviewDatabase();
            }

            if (database.Games == null)
            {
                database.Games =
                    new Dictionary<string, GameReview>();
            }

            return database;
        }

        // 특정 게임의 평가 불러오기
        public GameReview GetReview(Guid gameId)
        {
            var database = LoadDatabase();
            string key = gameId.ToString();

            if (database.Games.TryGetValue(key, out GameReview review))
            {
                if (review.Scores == null)
                {
                    review.Scores = new Dictionary<string, int>();
                }

                return review;
            }

            return new GameReview();
        }

        // 평가 존재 여부 확인
        public bool TryGetReview(Guid gameId, out GameReview review)
        {
            var database = LoadDatabase();
            string key = gameId.ToString();

            return database.Games.TryGetValue(key, out review);
        }

        // 평가 저장하기
        public void SaveReview(Game game, GameReview review)
        {
            var database = LoadDatabase();

            string key = game.Id.ToString();

            review.GameName = game.Name;
            review.LastEvaluatedAt = DateTime.UtcNow;

            if (review.Scores == null)
            {
                review.Scores = new Dictionary<string, int>();
            }

            database.Games[key] = review;

            SaveDatabase(database);

            // 저장한 평가에 사용된 ID도 캐시에 기록
            RememberCategoryIds(review.Scores.Keys);
        }

        // 전체 평가 데이터 저장
        private void SaveDatabase(ReviewDatabase database)
        {
            EnsureDirectoryExists();

            string json = Serialization.ToJson(database, true);

            File.WriteAllText(filePath, json);
        }

        // 지금까지 사용한 ID 목록 불러오기
        public List<string> GetKnownCategoryIds()
        {
            var cache = LoadCategoryIdCache();

            // 캐시에 누락된 ID가 있는 경우 게임 데이터에서도 수집
            var database = LoadDatabase();

            foreach (var review in database.Games.Values)
            {
                if (review?.Scores == null)
                {
                    continue;
                }

                foreach (string id in review.Scores.Keys)
                {
                    AddId(cache, id);
                }
            }

            SaveCategoryIdCache(cache);

            return cache.Ids
                .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        // 새로운 ID 목록을 기존 캐시에 추가
        public void RememberCategoryIds(IEnumerable<string> ids)
        {
            if (ids == null)
            {
                return;
            }

            var cache = LoadCategoryIdCache();

            foreach (string id in ids)
            {
                AddId(cache, id);
            }

            SaveCategoryIdCache(cache);
        }

        // 캐시 파일 불러오기
        private CategoryIdCache LoadCategoryIdCache()
        {
            if (!File.Exists(categoryCachePath))
            {
                return new CategoryIdCache();
            }

            var cache =
                Serialization.FromJsonFile<CategoryIdCache>(categoryCachePath);

            if (cache == null)
            {
                return new CategoryIdCache();
            }

            if (cache.Ids == null)
            {
                cache.Ids = new List<string>();
            }

            return cache;
        }

        // ID 추가 (중복 방지)
        private void AddId(CategoryIdCache cache, string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return;
            }

            id = id.Trim();

            if (!cache.Ids.Contains(id, StringComparer.Ordinal))
            {
                cache.Ids.Add(id);
            }
        }

        // 캐시 파일 저장
        private void SaveCategoryIdCache(CategoryIdCache cache)
        {
            EnsureDirectoryExists();

            string json = Serialization.ToJson(cache, true);

            File.WriteAllText(categoryCachePath, json);
        }

        // 데이터 폴더 존재 여부 확인
        private void EnsureDirectoryExists()
        {
            string directory = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        // ==========================================
        // 백업 및 복원
        // ==========================================

        // 평가 데이터 백업 파일 생성
        public void ExportBackup(string backupPath)
        {
            var database = LoadDatabase();
            var categoryCache = LoadCategoryIdCache();

            // 평가 데이터에 존재하는 ID도 백업에 포함
            foreach (var review in database.Games.Values)
            {
                if (review?.Scores == null)
                    continue;

                foreach (string id in review.Scores.Keys)
                {
                    AddId(categoryCache, id);
                }
            }

            var backup = new ReviewBackupData
            {
                BackupVersion = 1,
                Database = database,
                CategoryIds = categoryCache
            };

            string json = Serialization.ToJson(backup, true);

            File.WriteAllText(backupPath, json);
        }

        // 백업 파일에서 평가 데이터 복원
        public void ImportBackup(string backupPath)
        {
            // 먼저 백업 파일을 읽고 검사
            var backup =
                Serialization.FromJsonFile<ReviewBackupData>(backupPath);

            if (backup == null ||
                backup.BackupVersion != 1 ||
                backup.Database == null ||
                backup.Database.Games == null ||
                backup.CategoryIds == null ||
                backup.CategoryIds.Ids == null)
            {
                throw new InvalidDataException(
                    Loc.Get("LOC_CGR_Backup_InvalidFile"));
            }

            // 복원 데이터의 누락된 점수 딕셔너리 보정
            foreach (var review in backup.Database.Games.Values)
            {
                if (review != null && review.Scores == null)
                {
                    review.Scores = new Dictionary<string, int>();
                }
            }

            // 평가 데이터에 존재하는 ID도 캐시에 추가
            foreach (var review in backup.Database.Games.Values)
            {
                if (review?.Scores == null)
                    continue;

                foreach (string id in review.Scores.Keys)
                {
                    AddId(backup.CategoryIds, id);
                }
            }

            EnsureDirectoryExists();

            string directory = Path.GetDirectoryName(filePath);
            string suffix = Guid.NewGuid().ToString("N");

            string databaseTemp = Path.Combine(
                directory, "customRating_" + suffix + ".tmp");

            string categoryTemp = Path.Combine(
                directory, "categoryIds_" + suffix + ".tmp");

            string databaseBackup = Path.Combine(
                directory, "customRating_" + suffix + ".bak");

            string categoryBackup = Path.Combine(
                directory, "categoryIds_" + suffix + ".bak");

            bool hadDatabase = File.Exists(filePath);
            bool hadCategoryCache = File.Exists(categoryCachePath);

            try
            {
                // 실제 파일을 교체하기 전에 임시 파일에 기록
                File.WriteAllText(
                    databaseTemp,
                    Serialization.ToJson(backup.Database, true));

                File.WriteAllText(
                    categoryTemp,
                    Serialization.ToJson(backup.CategoryIds, true));

                // 기존 파일 보관
                if (hadDatabase)
                    File.Copy(filePath, databaseBackup);

                if (hadCategoryCache)
                    File.Copy(categoryCachePath, categoryBackup);

                // 새 데이터 적용
                File.Copy(databaseTemp, filePath, true);
                File.Copy(categoryTemp, categoryCachePath, true);
            }
            catch
            {
                // 복원 도중 실패하면 기존 데이터로 되돌림
                if (hadDatabase && File.Exists(databaseBackup))
                    File.Copy(databaseBackup, filePath, true);
                else if (!hadDatabase && File.Exists(filePath))
                    File.Delete(filePath);

                if (hadCategoryCache && File.Exists(categoryBackup))
                    File.Copy(categoryBackup, categoryCachePath, true);
                else if (!hadCategoryCache && File.Exists(categoryCachePath))
                    File.Delete(categoryCachePath);

                throw;
            }
            finally
            {
                // 임시 파일 정리
                DeleteIfExists(databaseTemp);
                DeleteIfExists(categoryTemp);
                DeleteIfExists(databaseBackup);
                DeleteIfExists(categoryBackup);
            }
        }

        private void DeleteIfExists(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    // 백업 파일에 저장할 데이터 구조
    public class ReviewBackupData
    {
        public int BackupVersion { get; set; }

        public ReviewDatabase Database { get; set; }

        public CategoryIdCache CategoryIds { get; set; }
    }
}