using MyRequest = Serenity.Services.ListRequest;
using MyResponse = Serenity.Services.ListResponse<SensorsReport.Frontend.Administration.LanguageRow>;
using MyRow = SensorsReport.Frontend.Administration.LanguageRow;


namespace SensorsReport.Frontend.Administration;
public interface ILanguageListHandler : IListHandler<MyRow, MyRequest, MyResponse> { }

public class LanguageListHandler(IRequestContext context) :
    ListRequestHandler<MyRow, MyRequest, MyResponse>(context), ILanguageListHandler
{
}