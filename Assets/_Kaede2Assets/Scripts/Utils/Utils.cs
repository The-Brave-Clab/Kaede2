using System.Collections;
using System.Globalization;
using System.Linq;
using Kaede2.Localization;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Kaede2.Utils
{
    public static class CommonUtils
    {
        public static string BytesToHumanReadable(double bytes)
        {
            string[] suffix = { "B", "KB", "MB", "GB", "TB" };
            int i = 0;
            double dblSByte = bytes;
            while (dblSByte >= 1024 * 0.8 && i < suffix.Length - 1)
            {
                dblSByte /= 1024;
                i++;
            }
            return $"{dblSByte:F2} {suffix[i]}";
        }

        public static bool BelongsTo(this CultureInfo thisCulture, CultureInfo thatCulture)
        {
            if (thatCulture == null)
                return false;
    
            if (Equals(thisCulture, CultureInfo.InvariantCulture))
                return Equals(thatCulture, CultureInfo.InvariantCulture);

            if (Equals(thatCulture, CultureInfo.InvariantCulture))
                return true;

            return thisCulture.Equals(thatCulture) || thisCulture.Parent.BelongsTo(thatCulture);
        }

        public static CultureInfo GetSystemLocaleOrDefault()
        {
            var locales = LocalizationManager.AllLocales;
            var locale = locales.FirstOrDefault(l => CultureInfo.CurrentCulture.BelongsTo(l)) ?? locales[0];
            typeof(CommonUtils).Log($"Selected system locale: {locale}");
            return locale;
        }

        // a mod function that works with negative numbers
        // Mod(-1, 3) == 2; Mod(1, 3) == 1
        public static int Mod(int x, int m)
        {
            if (m == 0) return x;
            return (x % m + m) % m;
        }

        public static void LoadNextScene(string sceneName, LoadSceneMode mode)
        {
            static IEnumerator LoadNextSceneCoroutine(string sceneName, LoadSceneMode mode)
            {
                yield return SceneTransition.Fade(1);
                yield return SceneManager.LoadSceneAsync(sceneName, mode);
            }

            CoroutineProxy.Start(LoadNextSceneCoroutine(sceneName, mode));
        }

        public static float GetScrollDiffToMakeItemVisible(this ScrollRect scrollRect, RectTransform item, float multiplier = 1.0f)
        {
            var viewport = scrollRect.viewport;
            var content = scrollRect.content;

            Vector3[] entryWorldCorners = new Vector3[4];
            item.GetWorldCorners(entryWorldCorners);
            var entryWorldSize = entryWorldCorners[2] - entryWorldCorners[0];

            Vector3[] viewportWorldCorners = new Vector3[4];
            viewport.GetWorldCorners(viewportWorldCorners);
            var viewportWorldSize = viewportWorldCorners[2] - viewportWorldCorners[0];

            Vector3[] contentWorldCorners = new Vector3[4];
            content.GetWorldCorners(contentWorldCorners);
            var contentWorldSize = contentWorldCorners[2] - contentWorldCorners[0];

            float topDiff = entryWorldCorners[2].y - (viewportWorldCorners[2].y - viewportWorldSize.y * multiplier);
            float bottomDiff = entryWorldCorners[0].y - (viewportWorldCorners[0].y + viewportWorldSize.y * multiplier);
            float posDiff = 0;
            if (topDiff > 0)
                posDiff = topDiff;
            else if (bottomDiff < 0)
                posDiff = bottomDiff;

            var scrollDiff = posDiff / (contentWorldSize.y - viewportWorldSize.y);
            return scrollDiff;
        }

        public static void MoveItemIntoViewport(this ScrollRect scrollRect, RectTransform item, float multiplier = 1.0f)
        {
            var scrollDiff = scrollRect.GetScrollDiffToMakeItemVisible(item, multiplier);
            scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition + scrollDiff);
        }

        public static Coroutine MoveItemIntoViewportSmooth(this ScrollRect scrollRect, RectTransform item, float duration = 0.2f, float multiplier = 1.0f)
        {
            return CoroutineProxy.Start(MoveItemIntoViewportSmoothCoroutine(scrollRect, item, duration, multiplier));
        }

        private static IEnumerator MoveItemIntoViewportSmoothCoroutine(ScrollRect scrollRect, RectTransform item, float duration, float multiplier)
        {
            var scrollDiff = scrollRect.GetScrollDiffToMakeItemVisible(item, multiplier);
            var startScrollPos = scrollRect.verticalNormalizedPosition;
            var targetScrollPos = Mathf.Clamp01(startScrollPos + scrollDiff);
            var elapsed = 0.0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                scrollRect.verticalNormalizedPosition = Mathf.Lerp(startScrollPos, targetScrollPos, elapsed / duration);
                yield return null;
            }
            scrollRect.verticalNormalizedPosition = targetScrollPos;
        }

        public static Color NoAlpha(this Color color)
        {
            return new Color(color.r, color.g, color.b, 1);
        }

        public static VertexGradient Multiply(this VertexGradient gradient, float scale)
        {
            return new VertexGradient(
                gradient.topLeft * scale,
                gradient.topRight * scale,
                gradient.bottomLeft * scale,
                gradient.bottomRight * scale);
        }

        public static VertexGradient LerpVertexGradient(VertexGradient a, VertexGradient b, float t)
        {
            return new VertexGradient(
                Color.Lerp(a.topLeft, b.topLeft, t),
                Color.Lerp(a.topRight, b.topRight, t),
                Color.Lerp(a.bottomLeft, b.bottomLeft, t),
                Color.Lerp(a.bottomRight, b.bottomRight, t));
        }

        public static VertexGradient NoAlpha(this VertexGradient gradient)
        {
            return new VertexGradient(
                gradient.topLeft.NoAlpha(),
                gradient.topRight.NoAlpha(),
                gradient.bottomLeft.NoAlpha(),
                gradient.bottomRight.NoAlpha());
        }

        public static Vector2Int GetMaxColumnRowCount(this GridLayoutGroup grid)
        {
            if (grid == null) return new Vector2Int(0, 0);
            RectTransform gridRT = grid.transform as RectTransform;
            if (gridRT == null) return new Vector2Int(0, 0);

            var gridWidth = gridRT.rect.width - grid.padding.left - grid.padding.right;
            var columnCount = Mathf.FloorToInt((gridWidth + grid.spacing.x) / (grid.cellSize.x + grid.spacing.x));

            int childCount = 0;
            for (var i = 0; i < grid.transform.childCount; i++)
            {
                if (grid.transform.GetChild(i).gameObject.activeSelf)
                    ++childCount;
            }

            var rowCount = Mathf.CeilToInt((float)childCount / columnCount);
            return new Vector2Int(columnCount, rowCount);
        }

        public static Vector2Int GetLocationFromChild(this GridLayoutGroup grid, Transform child)
        {
            if (grid == null || child == null) return new(-1, -1);
            if (!child.gameObject.activeSelf) return new(-1, -1);
            RectTransform gridRT = grid.transform as RectTransform;
            if (gridRT == null) return new(-1, -1);
            if (!child.IsChildOf(gridRT)) return new(-1, -1);
            // grid.width == grid.cellSize.x * grid.constraintCount + grid.spacing.x * (grid.constraintCount - 1) + grid.padding.left + grid.padding.right

            var maxCount = grid.GetMaxColumnRowCount();

            var goIndex = gridRT.Cast<Transform>().TakeWhile(c => c != child).Count();

            var row = goIndex / maxCount.x;
            var column = goIndex % maxCount.x;

            return new Vector2Int(column, row);
        }

        public static Transform GetChildFromLocation(this GridLayoutGroup grid, Vector2Int location)
        {
            if (grid == null) return null;
            RectTransform gridRT = grid.transform as RectTransform;
            if (gridRT == null) return null;

            var maxCount = grid.GetMaxColumnRowCount();

            int childCount = 0;
            for (var i = 0; i < grid.transform.childCount; i++)
            {
                if (grid.transform.GetChild(i).gameObject.activeSelf)
                    ++childCount;
            }

            var goIndex = location.y * maxCount.x + location.x;
            goIndex = Mathf.Clamp(goIndex, 0, childCount - 1);
            Transform child = null;
            for (var i = 0; i < grid.transform.childCount; i++)
            {
                var c = grid.transform.GetChild(i);
                if (!c.gameObject.activeSelf) continue;
                if (goIndex == 0)
                {
                    child = c;
                    break;
                }
                goIndex--;
            }

            return child;
        }
    }
}