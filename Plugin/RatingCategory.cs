using Playnite.SDK;
using System.Collections.Generic;

namespace CustomGameRating
{
    public class RatingCategory : ObservableObject
    {
        private string id = string.Empty;
        private string displayName = string.Empty;
        private int order;
        private bool enabled = true;

        public string Id
        {
            get => id;
            set => SetValue(ref id, value);
        }

        public string DisplayName
        {
            get => displayName;
            set => SetValue(ref displayName, value);
        }

        public int Order
        {
            get => order;
            set => SetValue(ref order, value);
        }

        public bool Enabled
        {
            get => enabled;
            set => SetValue(ref enabled, value);
        }
    }
}