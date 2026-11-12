
using Playnite.SDK.Controls;
using Playnite.SDK.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace CustomGameRating
{
    public partial class GameRatingBadgeControl : PluginUserControl
    {
        private readonly ReviewStore store;
        private readonly CustomGameRating plugin;

        private Game currentGame;
        private bool isSubscribed;

        public GameRatingBadgeControl(CustomGameRating plugin)
        {
            InitializeComponent();

            this.plugin = plugin;
            store = new ReviewStore(plugin);

            Loaded += OnControlLoaded;
            Unloaded += OnControlUnloaded;
        }

        public override void GameContextChanged(Game oldContext, Game newContext)
        {
            base.GameContextChanged(oldContext, newContext);

            currentGame = newContext;
            LoadScore(newContext);
        }

        private void LoadScore(Game game)
        {
            if (game == null)
            {
                ScoreText.Text = "";
                return;
            }

            try
            {
                if (!store.TryGetReview(game.Id, out GameReview review))
                {
                    ScoreText.Text = "No ★";
                    return;
                }

                var activeIds = plugin.CurrentSettings.Categories
                    .Where(category =>
                        category != null &&
                        category.Enabled &&
                        !string.IsNullOrWhiteSpace(category.Id))
                    .Select(category => category.Id.Trim())
                    .Distinct(StringComparer.Ordinal)
                    .ToList();

                int average = review.GetAverageScore(activeIds);

                ScoreText.Text = "★ " + average;
            }
            catch (Exception)
            {
                ScoreText.Text = "";
            }
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
                LoadScore(currentGame);
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

            Dispatcher.BeginInvoke(
                new Action(() => LoadScore(currentGame)),
                DispatcherPriority.Normal
            );
        }
    }
}
