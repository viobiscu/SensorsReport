using MyRow = SensorsReport.Frontend.SensorsReport.VariableTemplate.VariableTemplateRow;

namespace SensorsReport.Frontend.SensorsReport.VariableTemplate.Endpoints;
[Route("Services/SensorsReport/VariableTemplate/[action]")]
[ConnectionKey(typeof(MyRow)), ServiceAuthorize(typeof(MyRow))]
public class VariableTemplateEndpoint : ServiceEndpoint
{
    //[HttpPost, AuthorizeCreate(typeof(MyRow))]
    //public SaveResponse Create(IUnitOfWork uow, SaveRequest<MyRow> request, [FromServices] IVariableTemplateSaveHandler handler)
    //{
    //    return handler.Create(uow, request);
    //}

    //[HttpPost, AuthorizeUpdate(typeof(MyRow))]
    //public SaveResponse Update(IUnitOfWork uow, SaveRequest<MyRow> request, [FromServices] IVariableTemplateSaveHandler handler)
    //{
    //    return handler.Update(uow, request);
    //}

    //[HttpPost, AuthorizeDelete(typeof(MyRow))]
    //public DeleteResponse Delete(IUnitOfWork uow, DeleteRequest request, [FromServices] IVariableTemplateDeleteHandler handler)
    //{
    //    return handler.Delete(uow, request);
    //}

    public RetrieveResponse<MyRow> Retrieve(IDbConnection connection, RetrieveRequest request, [FromServices] IVariableTemplateRetrieveHandler handler)
    {
        return handler.Retrieve(connection, request);
    }

    public ListResponse<MyRow> List(IDbConnection connection, ListRequest request, [FromServices] IVariableTemplateListHandler handler)
    {
        return handler.List(connection, request);
    }
}
