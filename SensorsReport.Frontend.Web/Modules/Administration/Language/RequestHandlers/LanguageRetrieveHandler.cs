using MyRequest = Serenity.Services.RetrieveRequest;
using MyResponse = Serenity.Services.RetrieveResponse<SensorsReport.Frontend.Administration.LanguageRow>;
using MyRow = SensorsReport.Frontend.Administration.LanguageRow;


namespace SensorsReport.Frontend.Administration;
public interface ILanguageRetrieveHandler : IRetrieveHandler<MyRow, MyRequest, MyResponse> { }
public class LanguageRetrieveHandler(IRequestContext context) :
    RetrieveRequestHandler<MyRow, MyRequest, MyResponse>(context), ILanguageRetrieveHandler
{
}