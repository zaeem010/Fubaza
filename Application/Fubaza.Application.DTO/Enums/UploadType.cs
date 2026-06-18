using System.ComponentModel;

namespace Fubaza.Application.DTO.Enums
{
    public enum UploadType
    {

        [Description("player/requests")]
        PlayerRequest,

        [Description("club/requests")]
        ClubRequest,

        [Description("club_official/requests")]
        ClubOfficialRequest,

        [Description("templete/requests")]
        TempleteRequest,

        [Description("post/requests")]
        PostRequest,

        [Description("TempleteImage/requests")]
        TempleteImageRequest,

        [Description("MatchDay/Sponsor")]
        MatchDaySponsor
    }
}
