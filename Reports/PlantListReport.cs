using GrapeCity.ActiveReports;
using GrapeCity.ActiveReports.Document.Section;
using GrapeCity.ActiveReports.SectionReportModel;
using System.Drawing;

// Alias để tránh trùng tên
using ARFont = GrapeCity.ActiveReports.Document.Drawing.Font;
using ARFontStyle = GrapeCity.ActiveReports.Document.Drawing.FontStyle;

namespace SearchPlant.Reports
{
    public class PlantListReport : SectionReport
    {
        private int rowIndex = 0;
        public PlantListReport()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.PrintWidth = 8.4f;
            this.PageSettings.Margins.Top = 0.8f;
            this.PageSettings.Margins.Bottom = 0.8f;
            this.PageSettings.Margins.Left = 0.8f;
            this.PageSettings.Margins.Right = 0.8f;

            var pageHeader = new PageHeader { Height = 0.9f };
            var detail = new Detail { Height = 0.3f };
            var pageFooter = new PageFooter { Height = 0.5f };

            // --- Tiêu đề báo cáo ---
            var lblTitle = new Label
            {
                Text = "PLANT LIST REPORT",
                Location = new PointF(0, 0.1f),
                Size = new SizeF(this.PrintWidth, 0.5f),
                Font = new ARFont("Arial", 18, ARFontStyle.Bold),
                Alignment = TextAlignment.Center,
                ForeColor = Color.White,
                BackColor = Color.ForestGreen
            };
            pageHeader.Controls.Add(lblTitle);

            // --- Tiêu đề cột ---
            pageHeader.Controls.Add(CreateHeaderLabel("Plant Name", 0f, 1.9f));
            pageHeader.Controls.Add(CreateHeaderLabel("Scientific Name", 1.9f, 2.0f));
            pageHeader.Controls.Add(CreateHeaderLabel("Growth Cycle", 3.9f, 1.2f));
            pageHeader.Controls.Add(CreateHeaderLabel("Region", 5.1f, 1.8f));
            pageHeader.Controls.Add(CreateHeaderLabel("Last Update", 6.9f, 1.5f));

            // --- Dữ liệu chi tiết ---
            detail.Controls.Add(CreateDetailTextBox("PlantName", 0f, 1.9f));
            detail.Controls.Add(CreateDetailTextBox("ScientificName", 1.9f, 2.0f));
            detail.Controls.Add(CreateDetailTextBox("Cycle", 3.9f, 1.2f));
            detail.Controls.Add(CreateDetailTextBox("Region", 5.1f, 1.8f));
            detail.Controls.Add(CreateDetailTextBox("LastUpdate", 6.9f, 1.5f));

            // Sự kiện đổi màu nền xen kẽ

            // --- Footer: số trang ---
            var footerLabel = new Label
            {
                Text = "SearchPlant-Plant Report",
                Location = new PointF(0, 0.1f),
                Size = new SizeF(3f, 0.3f),
                Font = new ARFont("Arial", 9),
                ForeColor = Color.Gray
            };
            pageFooter.Controls.Add(footerLabel);

            this.Sections.Add(pageHeader);
            this.Sections.Add(detail);
            this.Sections.Add(pageFooter);
        }

        private Label CreateHeaderLabel(string text, float x, float width)
        {
            return new Label
            {
                Text = text,
                Location = new PointF(x, 0.6f),
                Size = new SizeF(width, 0.25f),
                Font = new ARFont("Arial", 10, ARFontStyle.Bold),
                VerticalAlignment = VerticalTextAlignment.Middle,
                ForeColor = Color.White,
                BackColor = Color.DarkGreen,
                Border = { Style = BorderLineStyle.Solid }
            };
        }

        private TextBox CreateDetailTextBox(string dataField, float x, float width)
        {
            return new TextBox
            {
                DataField = dataField,
                Location = new PointF(x, 0f),
                Size = new SizeF(width, 0.25f),
                Font = new ARFont("Arial", 10),
                VerticalAlignment = VerticalTextAlignment.Middle,
                Border = { Style = BorderLineStyle.Solid }
            };
        }

        // Đổi màu nền xen kẽ cho từng dòng
       
    }
}
