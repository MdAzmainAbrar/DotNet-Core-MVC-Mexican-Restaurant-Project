namespace MaxicanResturant.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId); //this is a read-only property that returns a boolean
                                                                       //value indicating whether the RequestId property has
                                                                       //a value or not. it uses the string.IsNullOrEmpty
                                                                       //method to check if the RequestId is null or an empty
                                                                       //string. if the RequestId has a value, ShowRequestId
                                                                       //will return true, otherwise it will return false. this
                                                                       //property can be used in the view to conditionally display
                                                                       //the RequestId if it is available. 
    }
}
