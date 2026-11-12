using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CustomGameRating
{
    public class CustomGameRating : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private CustomGameRatingSettingsViewModel settings { get; set; }

        public CustomGameRatingSettings CurrentSettings
        {
            get => settings.Settings;
        }

        public override Guid Id { get; } = Guid.Parse("447bc65f-ca7a-4b9d-8080-63d1b40e4e41");

        public CustomGameRating(IPlayniteAPI api) : base(api)
        {
            settings = new CustomGameRatingSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };

            AddCustomElementSupport(new AddCustomElementSupportArgs
            {
                SourceName = "CustomGameRating",
                ElementList = new List<string>
                {
                    "GameReviewControl",
                    "GameRatingBadgeControl"
                }
            });
        }

        public override void OnGameInstalled(OnGameInstalledEventArgs args)
        {
            // Add code to be executed when game is finished installing.
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            // Add code to be executed when Playnite is initialized.
        }

        public override void OnApplicationStopped(OnApplicationStoppedEventArgs args)
        {
            // Add code to be executed when Playnite is shutting down.
        }

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            // Add code to be executed when library is updated.
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new CustomGameRatingSettingsView();
        }

        public override IEnumerable<SidebarItem> GetSidebarItems()
        {
            yield return new SidebarItem
            {
                Title = Loc.Get("LOC_CGR_StatisticsTitle"),
                Type = SiderbarItemType.View,
                Icon = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "icon.png"),

                Opened = () => new RatingStatisticsView(this)
            };
        }

        public void OpenReview(Game game)
        {
            if (game == null)
            {
                return;
            }

            try
            {
                var store = new ReviewStore(this);
                var review = store.GetReview(game.Id);

                var window = API.Instance.Dialogs.CreateWindow(
                    new WindowCreationOptions
                    {
                        ShowMinimizeButton = true,
                        ShowMaximizeButton = false,
                        ShowCloseButton = true
                    }
                );

                // 2. 창 제목 및 크기 설정
                window.Title = Loc.Format("LOC_CGR_ReviewWindowTitle", game.Name);
                window.Width = 540;
                window.Height = 720;
                window.MinWidth = 440;
                window.MinHeight = 500;
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                window.ResizeMode = ResizeMode.CanResize;

                // 3. 우리가 수정한 UserControl을 창의 내용(Content)으로 주입
                window.Content = new ReviewWindow(
                    game,
                    review,
                    CurrentSettings.Categories,
                    (savedReview) =>
                    {
                        store.SaveReview(game, savedReview);
                        ReviewSaved?.Invoke(game.Id);
                    },
                    () => window.Close() // 닫기 동작 전달
                );

                window.ShowDialog();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    Loc.Format("LOC_CGR_ReviewLoadError", ex.Message),
                    Loc.Get("LOC_CGR_Error"),
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error
                );
            }
        }

        
        public override IEnumerable<GameMenuItem> GetGameMenuItems(
            GetGameMenuItemsArgs args)
        {
            if (args.Games == null || args.Games.Count != 1)
            {
                yield break;
            }

            yield return new GameMenuItem
            {
                Description = Loc.Get("LOC_CGR_MenuWriteReview"),
                Action = (menuArgs) =>
                {
                    OpenReview(menuArgs.Games[0]);
                }
            };
        }

        public override Control GetGameViewControl(
            GetGameViewControlArgs args)
        {
            if (args.Name == "GameReviewControl")
            {
                return new GameReviewControl(this);
            }

            if (args.Name == "GameRatingBadgeControl")
            {
                return new GameRatingBadgeControl(this);
            }

            return null;
        }

        public event Action<Guid> ReviewSaved;
        
    }
}
