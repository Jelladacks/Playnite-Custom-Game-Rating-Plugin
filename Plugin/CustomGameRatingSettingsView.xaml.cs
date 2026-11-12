using Microsoft.Win32;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CustomGameRating
{
    public partial class CustomGameRatingSettingsView : UserControl
    {
        public CustomGameRatingSettingsView()
        {
            InitializeComponent();
        }

        // 설정 객체 가져오기
        private CustomGameRatingSettings GetSettings()
        {
            var viewModel = DataContext as CustomGameRatingSettingsViewModel;

            if (viewModel == null || viewModel.Settings == null)
            {
                return null;
            }

            return viewModel.Settings;
        }

        // 현재 목록의 Order 값 갱신
        private void UpdateCategoryOrder()
        {
            var settings = GetSettings();

            if (settings == null)
            {
                return;
            }

            for (int i = 0; i < settings.Categories.Count; i++)
            {
                settings.Categories[i].Order = i;
            }
        }

        // 항목 추가
        private void AddCategory_Click(object sender, RoutedEventArgs e)
        {
            var settings = GetSettings();

            if (settings == null)
            {
                return;
            }

            var category = new RatingCategory
            {
                Id = System.Guid.NewGuid().ToString("N"),
                DisplayName = Loc.Get("LOC_CGR_Settings_NewCategoryName"),
                Order = settings.Categories.Count,
                Enabled = true
            };

            settings.Categories.Add(category);

            CategoryGrid.SelectedItem = category;
            CategoryGrid.ScrollIntoView(category);
        }

        // 항목 삭제
        private void RemoveCategory_Click(object sender, RoutedEventArgs e)
        {
            var settings = GetSettings();

            if (settings == null)
            {
                return;
            }

            var selected = CategoryGrid.SelectedItem as RatingCategory;

            if (selected == null)
            {
                MessageBox.Show(Loc.Get("LOC_CGR_Settings_SelectCategoryToRemove"));
                return;
            }

            var result = MessageBox.Show(
                Loc.Get("LOC_CGR_Settings_RemoveConfirm"),
                Loc.Get("LOC_CGR_Settings_RemoveTitle"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            settings.Categories.Remove(selected);

            UpdateCategoryOrder();
        }

        // 항목 위로 이동
        private void MoveUp_Click(object sender, RoutedEventArgs e)
        {
            var settings = GetSettings();

            if (settings == null)
            {
                return;
            }

            var selected = CategoryGrid.SelectedItem as RatingCategory;

            if (selected == null)
            {
                return;
            }

            int index = settings.Categories.IndexOf(selected);

            if (index <= 0)
            {
                return;
            }

            settings.Categories.Move(index, index - 1);

            UpdateCategoryOrder();

            CategoryGrid.SelectedItem = selected;
        }

        // 항목 아래로 이동
        private void MoveDown_Click(object sender, RoutedEventArgs e)
        {
            var settings = GetSettings();

            if (settings == null)
            {
                return;
            }

            var selected = CategoryGrid.SelectedItem as RatingCategory;

            if (selected == null)
            {
                return;
            }

            int index = settings.Categories.IndexOf(selected);

            if (index < 0 || index >= settings.Categories.Count - 1)
            {
                return;
            }

            settings.Categories.Move(index, index + 1);

            UpdateCategoryOrder();

            CategoryGrid.SelectedItem = selected;
        }

        // 평가 데이터 백업
        private void ExportBackup_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as CustomGameRatingSettingsViewModel;

            if (viewModel == null)
                return;

            var dialog = new SaveFileDialog
            {
                Title = Loc.Get("LOC_CGR_Backup_ExportTitle"),
                Filter = Loc.Get("LOC_CGR_Backup_FileFilter"),
                DefaultExt = ".json",
                AddExtension = true,
                FileName = "CustomGameRating_Backup.json",
                OverwritePrompt = true
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                viewModel.ExportBackup(dialog.FileName);

                MessageBox.Show(
                    Loc.Format("LOC_CGR_Backup_ExportDone", dialog.FileName),
                    Loc.Get("LOC_CGR_Backup_ExportDoneTitle"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    Loc.Format("LOC_CGR_Backup_ExportError", ex.Message),
                    Loc.Get("LOC_CGR_Backup_ExportErrorTitle"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // 평가 데이터 복원
        private void ImportBackup_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as CustomGameRatingSettingsViewModel;

            if (viewModel == null)
                return;

            var dialog = new OpenFileDialog
            {
                Title = Loc.Get("LOC_CGR_Backup_ImportTitle"),
                Filter = Loc.Get("LOC_CGR_Backup_FileFilter"),
                DefaultExt = ".json",
                CheckFileExists = true
            };

            if (dialog.ShowDialog() != true)
                return;

            var result = MessageBox.Show(
                Loc.Get("LOC_CGR_Backup_ImportConfirm"),
                Loc.Get("LOC_CGR_Backup_ImportConfirmTitle"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                viewModel.ImportBackup(dialog.FileName);

                MessageBox.Show(
                    Loc.Get("LOC_CGR_Backup_ImportDone"),
                    Loc.Get("LOC_CGR_Backup_ImportDoneTitle"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    Loc.Format("LOC_CGR_Backup_ImportError", ex.Message),
                    Loc.Get("LOC_CGR_Backup_ImportErrorTitle"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // ID 입력 중 자동완성 후보 표시
        private void CategoryIdComboBox_PreviewKeyUp(
            object sender,
            KeyEventArgs e)
        {
            var comboBox = sender as ComboBox;

            if (comboBox == null)
                return;

            // Enter, Escape, Tab은 기본 ComboBox 동작에 맡김
            if (e.Key == Key.Enter ||
                e.Key == Key.Escape ||
                e.Key == Key.Tab)
            {
                return;
            }

            comboBox.IsDropDownOpen =
                !string.IsNullOrWhiteSpace(comboBox.Text);
        }
    }
}
