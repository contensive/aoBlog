
using System.Linq;
using System.Text.RegularExpressions;

namespace Contensive.Blog.Controllers {
    public static class SpamDetector {
        //
        // ====================================================================================================
        /// <summary>
        /// Returns true if the comment appears to be spam based on pattern matching.
        /// Rules derived from analysis of historical spam vs approved comments.
        /// </summary>
        public static bool isSpam(string commenterName, string commentBody) {
            if (string.IsNullOrWhiteSpace(commentBody)) { return false; }
            string nameLower = (commenterName ?? "").Trim().ToLowerInvariant();
            string bodyLower = commentBody.Trim().ToLowerInvariant();
            //
            // -- name ends with "pic pic" (100% spam in historical data)
            if (nameLower.EndsWith("pic pic")) { return true; }
            //
            // -- body contains HTML anchor tags (100% spam in historical data)
            if (bodyLower.Contains("<a ") || bodyLower.Contains("<a\t") || bodyLower.Contains("<a href")) { return true; }
            //
            // -- body is just a URL with no other substantive text
            string bodyTrimmed = commentBody.Trim();
            if (Regex.IsMatch(bodyTrimmed, @"^\s*https?://\S+\s*$", RegexOptions.IgnoreCase)) { return true; }
            //
            // -- majority of characters are non-Latin (Cyrillic, Hebrew, Arabic, etc.)
            if (hasHighNonLatinRatio(bodyTrimmed)) { return true; }
            //
            // -- body contains 3 or more URLs
            int urlCount = Regex.Matches(bodyLower, @"https?://").Count;
            if (urlCount >= 3) { return true; }
            //
            return false;
        }
        //
        // ====================================================================================================
        /// <summary>
        /// Returns true if more than 50% of the letter characters in the text are non-Latin.
        /// </summary>
        private static bool hasHighNonLatinRatio(string text) {
            if (string.IsNullOrEmpty(text)) { return false; }
            int latinCount = 0;
            int nonLatinCount = 0;
            foreach (char c in text) {
                if (char.IsLetter(c)) {
                    if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')) {
                        latinCount++;
                    } else {
                        nonLatinCount++;
                    }
                }
            }
            int totalLetters = latinCount + nonLatinCount;
            if (totalLetters < 10) { return false; }
            return (double)nonLatinCount / totalLetters > 0.5;
        }
    }
}
