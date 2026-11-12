using Playnite.SDK;

namespace CustomGameRating
{
    // Localization 폴더의 언어 파일(en_US.xaml, ko_KR.xaml ...)에서 문자열을 가져오는 도우미
    internal static class Loc
    {
        public static string Get(string key)
        {
            return ResourceProvider.GetString(key);
        }

        // 키를 찾지 못하면 (ResourceProvider는 "<!키!>"를 돌려줌) fallback 사용
        public static string Get(string key, string fallback)
        {
            string value = ResourceProvider.GetString(key);

            return string.IsNullOrEmpty(value) || value.StartsWith("<!")
                ? fallback
                : value;
        }

        // "{0}" 같은 자리표시자가 있는 문자열
        public static string Format(string key, params object[] args)
        {
            return string.Format(Get(key), args);
        }
    }
}
