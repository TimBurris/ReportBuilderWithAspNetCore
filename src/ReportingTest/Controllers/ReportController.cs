using Microsoft.AspNetCore.Mvc;
using Microsoft.Reporting.NETCore;
using System.Text.Json;
using System.Text.Json.Serialization;

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
            try
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
            catch (Exception ex)
            {
                var options = new JsonSerializerOptions();
                options.Converters.Add(new ExceptionConverter<Exception>());

                return Content(System.Text.Json.JsonSerializer.Serialize<Exception>(ex, options));
            }
        }
    }
}
public class ExceptionConverter<TExceptionType> : JsonConverter<TExceptionType>
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(Exception).IsAssignableFrom(typeToConvert);
    }

    public override TExceptionType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException("Deserializing exceptions is not allowed");
    }

    public override void Write(Utf8JsonWriter writer, TExceptionType value, JsonSerializerOptions options)
    {
        var serializableProperties = value.GetType()
            .GetProperties()
            .Select(uu => new { uu.Name, Value = uu.GetValue(value) })
            .Where(uu => uu.Name != nameof(Exception.TargetSite));

        if (options?.IgnoreNullValues == true)
        {
            serializableProperties = serializableProperties.Where(uu => uu.Value != null);
        }

        var propList = serializableProperties.ToList();

        if (propList.Count == 0)
        {
            // Nothing to write
            return;
        }

        writer.WriteStartObject();

        foreach (var prop in propList)
        {
            writer.WritePropertyName(prop.Name);
            JsonSerializer.Serialize(writer, prop.Value, options);
        }

        writer.WriteEndObject();
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
