using System.Windows.Markup;
using System.IO;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CustomGameRating
{
    public class ReviewWindow : UserControl
    {
        private readonly Game game;
        private readonly Action<GameReview> onSave;
        private readonly Action closeAction; // 창 닫기용 델리게이트

        private readonly Dictionary<string, List<ScoreRow>> scoreRows =
            new Dictionary<string, List<ScoreRow>>();

        private readonly GameReview review;
        private TextBox reviewTextBox;

        private readonly List<string> activeCategoryIds;

        private TextBlock averageScoreText;

        public ReviewWindow(
            Game game,
            GameReview review,
            IEnumerable<RatingCategory> categories,
            Action<GameReview> onSave,
            Action closeAction)
        {
            this.game = game;
            this.review = review ?? new GameReview();
            this.onSave = onSave;
            this.closeAction = closeAction;

            // 커스텀 스타일 리소스 불러오기
            var styleUri = new Uri(
                "/CustomGameRating;component/ReviewWindowStyles.xaml",
                UriKind.Relative
            );

            Resources.MergedDictionaries.Add(
                new ResourceDictionary
                {
                    Source = styleUri
                }
            );

            if (this.review.Scores == null)
            {
                this.review.Scores = new Dictionary<string, int>();
            }

            var activeCategories = (categories ??
                Enumerable.Empty<RatingCategory>())
                .Where(category =>
                    category != null &&
                    category.Enabled &&
                    !string.IsNullOrWhiteSpace(category.Id))
                .OrderBy(category => category.Order)
                .ToList();

            activeCategoryIds = activeCategories
                .Select(category => category.Id.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToList();

            // Playnite 기본 배경색/폰트 설정 유지 (또는 테마 스타일 활용)
            // Background = new SolidColorBrush(Color.FromRgb(27, 29, 39));
            Foreground = new SolidColorBrush(Color.FromRgb(242, 242, 246));
            // FontFamily = new FontFamily("Segoe UI");

            // 전체 창의 최상위 컨테이너 (타이틀바를 제거했으므로 바로 body 역할을 겸하거나 DockPanel 구조 단순화)
            var body = new DockPanel
            {
                Margin = new Thickness(20)
            };

            // 하단 버튼
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 15, 0, 0)
            };

            var saveButton = new Button
            {
                Content = Loc.Get("LOC_CGR_Save"),
                Style = (Style)FindResource("ReviewSaveButton")
            };

            saveButton.Click += (sender, e) => Save();

            var cancelButton = new Button
            {
                Content = Loc.Get("LOC_CGR_Cancel"),
                Style = (Style)FindResource("ReviewButton")
            };

            // 취소 버튼 누르면 부모 창 닫기
            cancelButton.Click += (sender, e) => closeAction?.Invoke();

            buttonPanel.Children.Add(saveButton);
            buttonPanel.Children.Add(cancelButton);

            DockPanel.SetDock(buttonPanel, Dock.Bottom);
            body.Children.Add(buttonPanel);

            // 스크롤 가능한 내용
            var content = new StackPanel();

            content.Children.Add(new TextBlock
            {
                Text = game.Name,
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 20)
            });

            // 상세 평가 제목 + 종합 점수
            var ratingHeader = new Grid
            {
                Margin = new Thickness(0, 0, 0, 12)
            };

            ratingHeader.ColumnDefinitions.Add(
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            );

            ratingHeader.ColumnDefinitions.Add(
                new ColumnDefinition { Width = GridLength.Auto }
            );

            var sectionTitle = new TextBlock
            {
                Text = Loc.Get("LOC_CGR_Review_Detail"),
                FontSize = 15,
                FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center
            };

            averageScoreText = new TextBlock
            {
                Text = "★ —",
                FontSize = 21,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(255, 209, 102)),
                VerticalAlignment = VerticalAlignment.Center
            };

            Grid.SetColumn(sectionTitle, 0);
            Grid.SetColumn(averageScoreText, 1);

            ratingHeader.Children.Add(sectionTitle);
            ratingHeader.Children.Add(averageScoreText);

            content.Children.Add(ratingHeader);

            if (activeCategories.Count == 0)
            {
                content.Children.Add(new TextBlock
                {
                    Text = Loc.Get("LOC_CGR_Review_NoActiveCategories"),
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 10, 0, 15)
                });
            }

            foreach (var category in activeCategories)
            {
                string id = category.Id.Trim();
                int initialValue = 0;

                if (this.review.Scores.TryGetValue(id, out int savedScore))
                {
                    initialValue = savedScore;
                }

                AddScoreRow(content, category.DisplayName, id, initialValue);
            }

            UpdateAverageScore();

            content.Children.Add(new TextBlock
            {
                Text = Loc.Get("LOC_CGR_Review_ShortReview"),
                FontSize = 15,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 20, 0, 8)
            });

            reviewTextBox = new TextBox
            {
                Text = this.review.Review ?? "",
                Height = 140,
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Padding = new Thickness(10),
                BorderThickness = new Thickness(1),
                FontSize = 13,
                Style = (Style)FindResource("ReviewTextBox")
            };

            content.Children.Add(reviewTextBox);

            var scrollViewer = new ScrollViewer
            {
                Content = content,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
            };

            body.Children.Add(scrollViewer);

            // 최상단 루트에 body 배치
            Content = body;
        }

        // 평가 항목 한 줄 추가
        private void AddScoreRow(
            StackPanel parent,
            string label,
            string categoryId,
            int initialValue)
        {
            var row = new ScoreRow(
                label,
                initialValue,
                value => SetCategoryScore(categoryId, value),
                (Style)FindResource("ReviewSlider"),
                (Style)FindResource("ReviewCard")
            );

            parent.Children.Add(row.Container);

            if (!scoreRows.ContainsKey(categoryId))
            {
                scoreRows[categoryId] = new List<ScoreRow>();
            }

            scoreRows[categoryId].Add(row);
        }

        // 같은 ID를 가진 모든 항목의 점수를 동기화
        private void SetCategoryScore(string categoryId, int value)
        {
            if (!scoreRows.TryGetValue(categoryId, out var rows))
            {
                return;
            }

            foreach (var row in rows)
            {
                row.SetValue(value);
            }

            UpdateAverageScore();
        }

        // 현재 평가 항목들의 종합 점수 갱신
        private void UpdateAverageScore()
        {
            if (activeCategoryIds == null || activeCategoryIds.Count == 0)
            {
                averageScoreText.Text = "★ —";
                return;
            }

            var scores = activeCategoryIds
                .Where(id => scoreRows.ContainsKey(id) && scoreRows[id].Count > 0)
                .Select(id => scoreRows[id][0].Value)
                .ToList();

            if (scores.Count == 0)
            {
                averageScoreText.Text = "★ —";
                return;
            }

            int average = (int)Math.Round(
                scores.Average(),
                MidpointRounding.AwayFromZero
            );

            averageScoreText.Text = "★ " + average;
        }

        // 저장
        private void Save()
        {
            var result = new GameReview
            {
                GameName = game.Name,
                Review = reviewTextBox.Text,
                Scores = new Dictionary<string, int>()
            };

            foreach (string id in activeCategoryIds)
            {
                if (scoreRows.TryGetValue(id, out var rows) && rows.Count > 0)
                {
                    result.Scores[id] = rows[0].Value;
                }
            }

            foreach (var score in review.Scores)
            {
                if (!result.Scores.ContainsKey(score.Key))
                {
                    result.Scores[score.Key] = score.Value;
                }
            }

            try
            {
                onSave(result);
                closeAction?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    Loc.Format("LOC_CGR_Review_SaveError", ex.Message),
                    Loc.Get("LOC_CGR_Review_SaveErrorTitle"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        // 점수 입력 한 줄의 UI
        private class ScoreRow
        {
            public Grid Container { get; }
            public int Value => (int)Math.Round(slider.Value);

            private readonly Slider slider;
            private readonly TextBox valueText;
            private readonly Action<int> onValueChanged;

            private bool isUpdating;

            public ScoreRow(
                string label,
                int initialValue,
                Action<int> onValueChanged,
                Style sliderStyle,
                Style cardStyle)
            {
                this.onValueChanged = onValueChanged;

                Container = new Grid
                {
                    Margin = new Thickness(0, 5, 0, 5)
                };

                var cardContent = new Grid();

                cardContent.ColumnDefinitions.Add(
                    new ColumnDefinition { Width = new GridLength(110) });

                cardContent.ColumnDefinitions.Add(
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                cardContent.ColumnDefinitions.Add(
                    new ColumnDefinition { Width = new GridLength(48) });

                var title = new TextBlock
                {
                    Text = label,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = Brushes.White,
                    FontSize = 13,
                    FontWeight = FontWeights.Medium
                };

                slider = new Slider
                {
                    Minimum = 0,
                    Maximum = 100,
                    Value = Math.Max(0, Math.Min(100, initialValue)),
                    TickFrequency = 1,
                    IsSnapToTickEnabled = true,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = new SolidColorBrush(Color.FromRgb(214, 169, 77)),
                    Margin = new Thickness(8, 0, 8, 0),
                    Style = sliderStyle
                };

                valueText = new TextBox
                {
                    Text = ((int)slider.Value).ToString(),
                    Width = 42,
                    MaxLength = 3,
                    TextAlignment = TextAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Background = new SolidColorBrush(Color.FromRgb(41, 43, 55)),
                    Foreground = Brushes.White,
                    BorderBrush = new SolidColorBrush(Color.FromRgb(70, 72, 85)),
                    BorderThickness = new Thickness(1)
                };

                valueText.PreviewTextInput += (sender, e) =>
                {
                    string proposed = valueText.Text;
                    proposed = proposed.Remove(valueText.SelectionStart, valueText.SelectionLength);
                    proposed = proposed.Insert(valueText.SelectionStart, e.Text);
                    e.Handled = !IsValidInput(proposed);
                };

                DataObject.AddPastingHandler(valueText, (sender, e) =>
                {
                    string pastedText = "";
                    if (e.DataObject.GetDataPresent(DataFormats.UnicodeText))
                    {
                        pastedText = e.DataObject.GetData(DataFormats.UnicodeText) as string;
                    }

                    if (!IsValidInput(pastedText ?? ""))
                    {
                        e.CancelCommand();
                    }
                });

                valueText.TextChanged += (sender, e) =>
                {
                    if (isUpdating) return;
                    if (int.TryParse(valueText.Text, out int number))
                    {
                        onValueChanged(number);
                    }
                };

                slider.ValueChanged += (sender, e) =>
                {
                    if (isUpdating) return;
                    onValueChanged((int)Math.Round(slider.Value));
                };

                valueText.LostKeyboardFocus += (sender, e) =>
                {
                    if (int.TryParse(valueText.Text, out int number))
                    {
                        onValueChanged(number);
                    }
                    else
                    {
                        SetValue(Value);
                    }
                };

                Grid.SetColumn(title, 0);
                Grid.SetColumn(slider, 1);
                Grid.SetColumn(valueText, 2);

                cardContent.Children.Add(title);
                cardContent.Children.Add(slider);
                cardContent.Children.Add(valueText);

                var card = new Border
                {
                    Style = cardStyle,
                    Padding = new Thickness(12, 10, 12, 10),
                    Child = cardContent
                };

                Container.Children.Add(card);
            }

            public void SetValue(int value)
            {
                value = Math.Max(0, Math.Min(100, value));

                isUpdating = true;
                slider.Value = value;

                if (!valueText.IsKeyboardFocused)
                {
                    valueText.Text = value.ToString();
                }

                isUpdating = false;
            }

            private bool IsValidInput(string input)
            {
                if (!Regex.IsMatch(input, @"^\d{0,3}$"))
                {
                    return false;
                }

                if (int.TryParse(input, out int number))
                {
                    return number <= 100;
                }

                return true;
            }
        }
    }
}