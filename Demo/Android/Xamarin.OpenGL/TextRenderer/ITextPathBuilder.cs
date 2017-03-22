namespace Typography.Rendering
{
    interface ITextPathBuilder
    {
        void PathClear();
        void PathMoveTo(Point point);
        void PathLineTo(Point pos);
        void PathClose();
        void PathAddBezier(Point start, Point control, Point end);

        /// <summary>
        /// Append contour
        /// </summary>
        /// <param name="color"></param>
        void AddContour(Color color);
    }
}