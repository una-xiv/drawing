using System.Text;

namespace Una.Drawing.Font;

internal partial class DynamicFont
{
        /// <summary>
    /// Measures text, breaking lines based on maxWidth and explicit newline characters.
    /// </summary>
    /// <param name="text">The input text.</param>
    /// <param name="fontSize">The font size.</param>
    /// <param name="maxWidth">The maximum allowed width for a line.</param>
    /// <param name="lineHeightFactor">Factor to multiply the default line height by.</param>
    /// <returns>A MeasuredText object containing the lines and dimensions.</returns>
    private MeasuredText MeasureMultiLine(
        string text,
        int    fontSize,
        float  maxWidth,
        float  lineHeightFactor
    )
    {
        // Handle null or empty input gracefully
        if (string.IsNullOrEmpty(text)) {
             return new() {
                Lines     = [],
                Size      = new(0, 0),
                LineCount = 0,
             };
        }

        // Assuming GenerateChunks splits the text based on formatting changes,
        // but preserves newline characters within each chunk's Text.
        List<Chunk>  chunks = GenerateChunks(text);
        List<string> lines  = [];

        float lineHeight       = GetLineHeight(fontSize);
        float currentLineWidth = 0;
        float maxLineWidth     = 0;
        float totalHeight      = 0;
        var   lineBuffer       = new StringBuilder();

        // Process chunk by chunk to handle potential font changes
        foreach (var chunk in chunks)
        {
            // Get font and space width for the current chunk
            SKFont chunkFont = GetFont(chunk, fontSize);
            // Measure space width using the specific font for this chunk
            float spaceWidth = chunkFont.MeasureText(" ");

            // Split the chunk's text by newline characters first.
            // StringSplitOptions.None ensures that sequences like "\n\n" result in empty strings,
            // representing blank lines that should be preserved.
            string[] subLines = chunk.Text.Split(["\n"], StringSplitOptions.None);

            for (int i = 0; i < subLines.Length; i++)
            {
                string subLine = subLines[i];

                // Split the current sub-line (part between newlines) into words based on spaces.
                // RemoveEmptyEntries ensures we don't get empty strings from multiple spaces,
                // TrimEntries removes leading/trailing whitespace from words.
                string[] words = subLine.Split([' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                // Process words for width-based wrapping
                foreach (string word in words)
                {
                    // Re-measure word width with the potentially chunk-specific font
                    float wordWidth = chunkFont.MeasureText(word);
                    float widthWithNextWord = currentLineWidth;

                    // Add space width if the line isn't empty
                    if (lineBuffer.Length > 0)
                    {
                        widthWithNextWord += spaceWidth;
                    }
                    widthWithNextWord += wordWidth;

                    // --- Width-based line break ---
                    // Check if adding the next word exceeds the max width.
                    // Using >= maxWidth ensures we break *before* exceeding. Add a small tolerance (e.g., 1f)
                    // if floating point inaccuracies might cause issues near the boundary.
                    if (lineBuffer.Length > 0 && widthWithNextWord + 1f >= maxWidth)
                    {
                        // Current line is full, add it to the list
                        string completedLine = lineBuffer.ToString();
                        lines.Add(completedLine);
                        totalHeight  += (lineHeight * lineHeightFactor);
                        maxLineWidth =  MathF.Max(maxLineWidth, currentLineWidth);

                        // Start a new line with the current word
                        lineBuffer.Clear().Append(word);
                        currentLineWidth = wordWidth;
                        continue; // Skip to the next word
                    }

                    // --- Append word to current line ---
                    // If it's not the first word on the line, add a space first.
                    if (lineBuffer.Length > 0)
                    {
                        lineBuffer.Append(' ');
                        currentLineWidth += spaceWidth;
                    }

                    // Append the word itself
                    lineBuffer.Append(word);
                    currentLineWidth += wordWidth;
                } // End foreach word in subLine

                // --- Newline-based line break ---
                // After processing all words in a subLine (or if the subLine was empty),
                // check if a newline character (\n) caused this subLine to end.
                // This is true if it's not the *last* subLine generated by splitting this chunk's text.
                if (i < subLines.Length - 1)
                {
                    // A newline was encountered, so force a line break regardless of width.
                    // Add the content accumulated so far (from the subLine just processed).
                    string completedLine = lineBuffer.ToString();
                    lines.Add(completedLine);
                    totalHeight  += (lineHeight * lineHeightFactor);
                    maxLineWidth =  MathF.Max(maxLineWidth, currentLineWidth);

                    // Reset for the next line (which starts conceptually after the \n)
                    lineBuffer.Clear();
                    currentLineWidth = 0;
                }
                // If it *was* the last subLine of the chunk (i == subLines.Length - 1),
                // the lineBuffer content carries over either to the next chunk
                // or to the final flush after all chunks are processed.

            } // End for each subLine
        } // End foreach chunk

        // Add any remaining text in the buffer as the very last line
        if (lineBuffer.Length > 0)
        {
            string lastLine = lineBuffer.ToString();
            lines.Add(lastLine);
            totalHeight += (lineHeight * lineHeightFactor);
            maxLineWidth = MathF.Max(maxLineWidth, currentLineWidth);
        }
        // Handle the case where the input text ends with a newline,
        // which might result in an empty lineBuffer after the loops,
        // but the last forced break (`if (i < subLines.Length - 1)`) correctly added the line *before* the final empty segment.
        // Exception: If the *only* content was say "\n", the loop adds one empty line, totalHeight is updated once.
        // If the text was completely empty or just whitespace removed by trimming, lines will be empty.
        else if (lines.Count == 0 && !string.IsNullOrEmpty(text))
        {
             // If after all processing, no lines were added BUT the original text wasn't empty
             // (e.g., text was "\n" or just spaces/newlines), add a single empty line.
             // The loop structure should handle "\n" correctly already by adding one line.
             // This case might be redundant but acts as a fallback. Consider if needed based on desired behavior for " ".
             // If GetLineHeight or lineHeightFactor is 0, totalHeight might be 0. Ensure a minimum height?
             // Let's assume GetLineHeight returns a positive value.
        }


        // Ensure totalHeight reflects at least one line if any lines were generated,
        // especially if the last line added via buffer flush didn't increment height yet.
        // The current logic correctly adds height for the final buffered line *if* the buffer wasn't empty.
        // If the result is exactly one line, totalHeight should be lineHeight * lineHeightFactor.
        // If lines.Count > 0 and totalHeight = 0 (unlikely unless factor/height is 0), adjust:
        if (lines.Count > 0 && totalHeight <= 0)
        {
             totalHeight = lineHeight * lineHeightFactor; // Ensure at least one line height
        }


        return new() {
            Lines     = lines.ToArray(),
            // Use MathF.Ceiling to ensure the size covers the content.
            // Ensure width is at least 1 pixel if there's content, otherwise 0.
            Size      = new(lines.Count > 0 ? MathF.Ceiling(MathF.Max(1, maxLineWidth)) : 0, MathF.Ceiling(totalHeight)),
            LineCount = (uint)lines.Count,
        };
    }
}