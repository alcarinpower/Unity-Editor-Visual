using UnityEngine;
using UnityEngine.UIElements;

namespace CodeDestroyer.Editor.EditorVisual.UIElements
{
    /// <summary>
    /// A custom visual element that represents a line. This line can be either horizontal or vertical, and its length and color can be customized.
    /// </summary>
    [UxmlElement]
    public partial class Line : VisualElement
    {

        /// <summary>
        /// Creates a horizontal line with the default height of 1 unit.
        /// </summary>
        public Line()
        {
            style.height = 1;
            style.width = Length.Percent(100f);
            style.backgroundColor = GlobalVariables.DefaultLineColor;
        }

        /// <summary>
        /// Creates a line with a customizable length and orientation (horizontal or vertical). The length is clamped between 1 and 200 units, and the color matches Unity's default line color.
        /// </summary>
        /// <param name="lineLength">The length of the line. Minimum value is 1f, and maximum is 200f. The line will be clamped within this range.</param>
        /// <param name="isVertical">Determines if the line is vertical. If false, the line will be horizontal.</param>
        public Line(float lineLength = 1f, bool isVertical = false)
        {
            style.backgroundColor = GlobalVariables.DefaultLineColor;

            lineLength = Mathf.Clamp(lineLength, 1f, 200f);

            if (isVertical)
            {
                style.width = lineLength;
            }
            else
            {
                style.height = lineLength;
            }

        }

        /// <summary>
        /// Creates a line with a customizable length, orientation (horizontal or vertical), and color. If no color is specified, the default line color is used.
        /// The length is clamped between 1 and 200 units.
        /// </summary>
        /// <param name="lineLength">The length of the line. Minimum value is 1f, and maximum is 200f. The line will be clamped within this range.</param>
        /// <param name="isVertical">Determines if the line is vertical. If false, the line will be horizontal.</param>
        /// <param name="lineColor">The color of the line. If not specified, the default color is used.</param>
        public Line(float lineLength = 1f, bool isVertical = true, Color lineColor = default)
        {
            if (lineColor != null || lineColor != default)
            {
                style.backgroundColor = lineColor;
            }
            else
            {
                style.backgroundColor = lineColor;
            }

            lineLength = Mathf.Clamp(lineLength, 1f, 200f);

            if (isVertical)
            {
                style.width = lineLength;
                //style.height = Length.Percent(100f);

            }
            else
            {
                style.height = lineLength;
                //style.width = Length.Percent(100f);
            }
        }
    }
}
