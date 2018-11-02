namespace Maha.JsonService
{
    public delegate JsonRpcException PreProcessHandler(JsonRequest request, object context);

    public delegate void CompletedProcessHandler(JsonRequest request, JsonResponse response, object context);
}
