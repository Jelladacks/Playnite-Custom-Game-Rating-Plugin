using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomGameRating
{
    public class CustomGameRatingSettings : ObservableObject
    {
        private ObservableCollection<RatingCategory> categories =
            new ObservableCollection<RatingCategory>();

        public ObservableCollection<RatingCategory> Categories
        {
            get => categories;
            set => SetValue(ref categories, value);
        }
    }

    public class CustomGameRatingSettingsViewModel : ObservableObject, ISettings
    {
        private readonly CustomGameRating plugin;
        private CustomGameRatingSettings editingClone { get; set; }

        private CustomGameRatingSettings settings;
        public CustomGameRatingSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<string> knownCategoryIds =
            new ObservableCollection<string>();

        public ObservableCollection<string> KnownCategoryIds
        {
            get => knownCategoryIds;
            set => SetValue(ref knownCategoryIds, value);
        }

        public CustomGameRatingSettingsViewModel(CustomGameRating plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<CustomGameRatingSettings>();

            // LoadPluginSettings returns null if no saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;

                if (Settings.Categories == null)
                {
                    Settings.Categories =
                        new System.Collections.ObjectModel.ObservableCollection<RatingCategory>();
                }
            }
            else
            {
                Settings = new CustomGameRatingSettings();

                Settings.Categories.Add(new RatingCategory
                {
                    Id = "story",
                    DisplayName = Loc.Get("LOC_CGR_DefaultCategory_Story", "Story"),
                    Order = 0,
                    Enabled = true
                });

                Settings.Categories.Add(new RatingCategory
                {
                    Id = "gameplay",
                    DisplayName = Loc.Get("LOC_CGR_DefaultCategory_Gameplay", "Gameplay"),
                    Order = 1,
                    Enabled = true
                });

                Settings.Categories.Add(new RatingCategory
                {
                    Id = "graphics",
                    DisplayName = Loc.Get("LOC_CGR_DefaultCategory_Graphics", "Graphics"),
                    Order = 2,
                    Enabled = true
                });

                Settings.Categories.Add(new RatingCategory
                {
                    Id = "sound",
                    DisplayName = Loc.Get("LOC_CGR_DefaultCategory_Sound", "Sound"),
                    Order = 3,
                    Enabled = true
                });

                Settings.Categories.Add(new RatingCategory
                {
                    Id = "direction",
                    DisplayName = Loc.Get("LOC_CGR_DefaultCategory_Direction", "Direction"),
                    Order = 4,
                    Enabled = true
                });
            }

            // 기존에 사용했던 평가 항목 ID 불러오기
            var store = new ReviewStore(plugin);

            var knownIds = store.GetKnownCategoryIds();

            // 현재 설정에 등록된 ID도 자동완성 후보에 포함
            foreach (var category in Settings.Categories)
            {
                if (string.IsNullOrWhiteSpace(category.Id))
                    continue;

                string id = category.Id.Trim();

                if (!knownIds.Contains(id, StringComparer.Ordinal))
                    knownIds.Add(id);
            }

            KnownCategoryIds =
                new ObservableCollection<string>(knownIds);
        }

        public void BeginEdit()
        {
            // Code executed when settings view is opened and user starts editing values.
            editingClone = Serialization.GetClone(Settings);
        }

        public void CancelEdit()
        {
            // Code executed when user decides to cancel any changes made since BeginEdit was called.
            // This method should revert any changes made to Option1 and Option2.
            Settings = editingClone;
        }

        public void EndEdit()
        {
            var store = new ReviewStore(plugin);

            store.RememberCategoryIds(
                Settings.Categories.Select(category => category.Id)
            );

            // 방금 저장한 ID까지 자동완성 목록 갱신
            KnownCategoryIds =
                new ObservableCollection<string>(
                    store.GetKnownCategoryIds()
                );

            plugin.SavePluginSettings(Settings);
        }

        // 평가 데이터 백업
        public void ExportBackup(string path)
        {
            var store = new ReviewStore(plugin);
            store.ExportBackup(path);
        }

        // 평가 데이터 복원
        public void ImportBackup(string path)
        {
            var store = new ReviewStore(plugin);
            store.ImportBackup(path);
        }

        public bool VerifySettings(out List<string> errors)
        {
            // Code execute when user decides to confirm changes made since BeginEdit was called.
            // Executed before EndEdit is called and EndEdit is not called if false is returned.
            // List of errors is presented to user if verification fails.
            errors = new List<string>();
            return true;
        }


    }
}