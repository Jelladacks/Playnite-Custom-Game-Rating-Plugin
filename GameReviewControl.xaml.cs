
using Playnite.SDK.Controls;
using Playnite.SDK.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CustomGameRating
{
    public partial class GameReviewControl : PluginUserControl
    {
        private readonly ReviewStore store;
        private readonly CustomGameRating plugin;

        private Game currentGame;
        private bool isSubscribed;

        public GameReviewControl(CustomGameRating plugin)
        {
            InitializeComponent();

            this.plugin = plugin;
            store = new ReviewStore(plugin);

            Loaded += OnControlLoaded;
            Unloaded += OnControlUnloaded;
        }

        public override void GameContextChanged(
            Game oldContext,
            Game newContext)
        {
            base.GameContextChanged(oldContext, newContext);

            currentGame = newContext;
            LoadReview(newContext);
        }

        private void LoadReview(Game game)
        {
            if (game == null)
            {
                ShowEmptyMessage();
                return;
            }

            try
            {
                if (!store.TryGetReview(game.Id, out GameReview review))
                {
                    ShowEmptyMessage();
                    return;
                }

                if (review.Scores == null)
                {
                    review.Scores =
                        new System.Collections.Generic.Dictionary<string, int>();
                }

                var activeCategories = plugin.CurrentSettings.Categories
                    .Where(category =>
                        category != null &&
                        category.Enabled &&
                        !string.IsNullOrWhiteSpace(category.Id))
                    .OrderBy(category => category.Order)
                    .ToList();

                var activeIds = activeCategories
                    .Select(category => category.Id.Trim())
                    .Distinct(StringComparer.Ordinal)
                    .ToList();

                // 종합 점수
                AverageScoreText.Text =
                    "★ " + review.GetAverageScore(activeIds);

                // 기존 행 제거
                CategoryRows.Children.Clear();

                // 설정에 등록된 항목을 동적으로 생성
                foreach (var category in activeCategories)
                {
                    string id = category.Id.Trim();

                    bool hasScore =
                        review.Scores.TryGetValue(id, out int score);

                    AddCategoryRow(
                        category.DisplayName,
                        hasScore ? score : 0,
                        hasScore
                    );
                }

                // 후기
                ReviewText.Text = string.IsNullOrWhiteSpace(review.Review)
                    ? Loc.Get("LOC_CGR_Review_NoReviewText")
                    : review.Review;

                EmptyMessage.Visibility = Visibility.Collapsed;
                ReviewContent.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                ReviewContent.Visibility = Visibility.Collapsed;

                EmptyMessage.Content = new TextBlock
                {
                    Text = Loc.Format("LOC_CGR_Review_LoadFailed", ex.Message),
                    TextWrapping = TextWrapping.Wrap,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                EmptyMessage.Visibility = Visibility.Visible;
                EmptyMessage.IsEnabled = false;
            }
        }

        // 평가 항목 한 줄 생성
        private void AddCategoryRow(
            string label,
            int score,
            bool hasScore)
        {
            var row = new Grid
            {
                Margin = new Thickness(0, 5, 0, 5)
            };

            row.ColumnDefinitions.Add(
                new ColumnDefinition { Width = new GridLength(90) });

            row.ColumnDefinitions.Add(
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            row.ColumnDefinitions.Add(
                new ColumnDefinition { Width = new GridLength(45) });

            var title = new TextBlock
            {
                Text = label,
                VerticalAlignment = VerticalAlignment.Center,
                Style = (Style)FindResource("BaseTextBlockStyle")
            };

            var bar = new ProgressBar
            {
                Minimum = 0,
                Maximum = 100,
                Value = score,
                Height = 8,
                VerticalAlignment = VerticalAlignment.Center,
                Background = Brushes.Transparent,
                Foreground = (System.Windows.Media.Brush)
                    FindResource("GlyphBrush")
            };

            var scoreText = new TextBlock
            {
                Text = hasScore ? score.ToString() : "—",
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Style = (Style)FindResource("BaseTextBlockStyle")
            };

            Grid.SetColumn(title, 0);
            Grid.SetColumn(bar, 1);
            Grid.SetColumn(scoreText, 2);

            row.Children.Add(title);
            row.Children.Add(bar);
            row.Children.Add(scoreText);

            CategoryRows.Children.Add(row);
        }

        private void ShowEmptyMessage()
        {
            ReviewContent.Visibility = Visibility.Collapsed;

            EmptyMessage.Visibility = Visibility.Visible;
            EmptyMessage.IsEnabled = currentGame != null;
        }

        private void EmptyMessage_Click(object sender, RoutedEventArgs e)
        {
            if (currentGame == null)
            {
                return;
            }

            plugin.OpenReview(currentGame);
        }

        private void OnControlLoaded(object sender, RoutedEventArgs e)
        {
            if (!isSubscribed)
            {
                plugin.ReviewSaved += OnReviewSaved;
                isSubscribed = true;
            }

            if (currentGame != null)
            {
                LoadReview(currentGame);
            }
        }

        private void OnControlUnloaded(object sender, RoutedEventArgs e)
        {
            if (isSubscribed)
            {
                plugin.ReviewSaved -= OnReviewSaved;
                isSubscribed = false;
            }
        }

        private void OnReviewSaved(Guid gameId)
        {
            if (currentGame == null || currentGame.Id != gameId)
            {
                return;
            }

            if (Dispatcher.CheckAccess())
            {
                LoadReview(currentGame);
            }
            else
            {
                Dispatcher.BeginInvoke(
                    new Action(() => LoadReview(currentGame))
                );
            }
        }
    }
}
