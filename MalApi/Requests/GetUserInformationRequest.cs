namespace MalApi.Requests
{
    public class GetUserInformationRequest : HttpGetRequest<MalUser>
    {
        public override string BaseUrl => "https://api.myanimelist.net/v2/users/@me";

        public GetUserInformationRequest()
        {
            Parameters.Add("fields", "anime_statistics");
        }
    }
}
