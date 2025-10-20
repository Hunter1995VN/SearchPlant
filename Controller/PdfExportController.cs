using Microsoft.AspNetCore.Mvc;
using GrapeCity.ActiveReports;
using GrapeCity.ActiveReports.Export.Pdf.Section;
using GrapeCity.ActiveReports.Export.Excel.Section;
using System.IO;
using System.Threading.Tasks;
using SearchPlant.Models;
using Microsoft.EntityFrameworkCore; // ✅ Bắt buộc phải có using này cho .Include()
using SearchPlant.Reports;
using System;
using System.Linq;
namespace SearchPlant.Controllers
{
    [Route("api/PdfExport")]
    [ApiController]
    public class ReportExportController : ControllerBase
    {
        private readonly SearchPlantContext _context;

        public ReportExportController(SearchPlantContext context)
        {
            _context = context;
        }

        [HttpGet("download-plant-report")]
        public async Task<IActionResult> DownloadReport([FromQuery] string format = "pdf",
    [FromQuery] DateTime? startDate = null,
    [FromQuery] DateTime? endDate = null)
        {
            try
            {
                // ✅ SỬA LỖI: Thêm .Include() để tải dữ liệu liên quan
                // 1. Bắt đầu xây dựng câu truy vấn dưới dạng IQueryable
                var plantsQuery = _context.Plants
                    .Include(p => p.Cycle)
                    .Include(p => p.Regions)
                    .AsQueryable(); // AsQueryable để có thể thêm điều kiện linh hoạt

                // ✅ THÊM LOGIC LỌC THEO NGÀY THÁNG VÀO ĐÂY
                // 2. Nếu người dùng chọn ngày bắt đầu, thêm điều kiện lọc
              if (startDate.HasValue)
        {
            // ✅ SỬA LỖI: Chuyển đổi DateTime sang UTC trước khi sử dụng
            // Điều này đảm bảo Kind của DateTime là Utc, tương thích với PostgreSQL.
            var utcStartDate = DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc);
            plantsQuery = plantsQuery.Where(p => p.Lastupdated >= utcStartDate);
        }

        if (endDate.HasValue)
        {
            // ✅ SỬA LỖI: Tương tự, chuyển đổi endDate sang UTC
            var utcEndDate = DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc);
            
            // Lấy thời điểm cuối cùng của ngày đó (nhỏ hơn ngày kế tiếp)
            var nextDay = utcEndDate.Date.AddDays(1);
            plantsQuery = plantsQuery.Where(p => p.Lastupdated < nextDay);
        }

                // 4. Sau khi đã áp dụng tất cả các bộ lọc, mới thực thi truy vấn
                var plantData = await plantsQuery.ToListAsync();
               var reportData = plantData.Select(p => new 
{
    PlantName = p.Plantname,
    ScientificName = p.Scientificname,
    Cycle = p.Cycle != null ? p.Cycle.Cyclename : string.Empty,
    Region = string.Join(", ", p.Regions.Select(r => r.Regionname)),LastUpdate = p.Lastupdated
}).ToList();


                var report = new PlantListReport();
                // ✅ Sử dụng dữ liệu đã được làm phẳng
                report.DataSource = reportData;
                report.Run();

                var memStream = new MemoryStream();
                string mimeType;
                string fileExtension;

                switch (format.ToLower())
                {
                    case "excel":
                        var xlsExport = new XlsExport();
                        xlsExport.Export(report.Document, memStream);
                        mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        fileExtension = "xlsx";
                        break;
                    
                    case "pdf":
                    default:
                        var pdfExport = new PdfExport();
                        pdfExport.Export(report.Document, memStream);
                        mimeType = "application/pdf";
                        fileExtension = "pdf";
                        break;
                }

                memStream.Position = 0;
                return File(memStream, mimeType, $"PlantListReport.{fileExtension}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi tạo báo cáo: {ex.ToString()}");
            }
        }
    }
}