using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace VisualHFT.Helpers
{
    class ProgramPaginator : DocumentPaginator
    {
        private FrameworkElement Element;
        private ProgramPaginator()
        {
        }

        public ProgramPaginator(FrameworkElement element)
        {
            Element = element;
        }

        public override DocumentPage GetPage(int pageNumber)
        {

            Element.RenderTransform = new TranslateTransform(-PageSize.Width * (pageNumber % Columns), -PageSize.Height * (pageNumber / Columns));

            Size elementSize = new Size(
                Element.ActualWidth,
                Element.ActualHeight);
            Element.Measure(elementSize);
            Element.Arrange(new Rect(new Point(0, 0), elementSize));

            var page = new DocumentPage(Element);
            Element.RenderTransform = null;

            return page;
        }

        public override bool IsPageCountValid
        {
            get { return true; }
        }

        public int Columns
        {
            get
            {
                return (int)Math.Ceiling(Element.ActualWidth / PageSize.Width);
            }
        }
        public int Rows
        {
            get
            {
                return (int)Math.Ceiling(Element.ActualHeight / PageSize.Height);
            }
        }

        public override int PageCount
        {
            get
            {
                return Columns * Rows;
            }
        }

        public override Size PageSize
        {
            set;
            get;
        }

        public override IDocumentPaginatorSource Source
        {
            get { return null; }
        }
    }
}
