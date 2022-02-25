using Microsoft.AspNetCore.Mvc;
using Microsoft.Reporting.NETCore;
namespace ReportingTest.Controllers
{
    [Route("[controller]")]
    public class ReportController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ReportController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            string filePath = Path.Join(_webHostEnvironment.ContentRootPath, "Reports\\ReportAlpha.rdl");
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                List<PurchaseOrder> purchaseOrders = new List<PurchaseOrder>()
                {
                    new PurchaseOrder()
                    {
                        CustomerName="Jack Sparrow",
                         PurchaseOrderNumber="8675309",
                    }
                };

                List<LineItem> lineItems = new List<LineItem>() {
                    new LineItem()
                    {
                        Amount=(53.59).ToString("C"),
                        Material="Steel Tubing"
                    },
                    new LineItem()
                    {
                        Amount=(500).ToString("C"),
                        Material="Stuff"
                    }
                };
                LocalReport report = new();
                report.LoadReportDefinition(fileStream);
                report.DataSources.Add(new ReportDataSource(name: "PurchaseOrder", purchaseOrders));
                report.DataSources.Add(new ReportDataSource(name: "LineItem", lineItems));

                byte[] pdfData = report.Render(format: "PDF");

                return File(pdfData, contentType: "application/pdf");
            }
        }
    }
}

public class PurchaseOrder
{
    public string PurchaseOrderNumber { get; set; }
    public string CustomerName { get; set; }
}

public class LineItem
{
    public string Amount { get; set; }
    public string Material { get; set; }
}
