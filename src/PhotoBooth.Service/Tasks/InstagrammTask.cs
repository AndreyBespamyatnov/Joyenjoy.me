namespace PhotoBooth.Service.Tasks
{
    public class InstagrammTask
    {

        //Check count of images by tag: https://api.instagram.com/v1/tags/cosmo9?access_token=2157679132.d64d638.eb9e10a9066349619c0298a2fa640b6f
        //regex: media_count":(\d*)

        //if we have media by tag, for example: https://api.instagram.com/v1/tags/cosmo9/media/recent?access_token=2157679132.d64d638.eb9e10a9066349619c0298a2fa640b6f
        //standard_resolution":\{"url":"(.*?)","width
        //if next_url":"(.*?)"\},"meta" return result, grab first link
    }
}