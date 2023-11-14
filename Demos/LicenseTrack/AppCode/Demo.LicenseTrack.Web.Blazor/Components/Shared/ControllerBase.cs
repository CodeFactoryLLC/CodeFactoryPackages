using Demo.LicenseTrack.App.Model;
using Microsoft.AspNetCore.Components;

namespace Demo.LicenseTrack.Web.Blazor.Components.Shared
{
    /// <summary>
    /// Base class for controllers
    /// </summary>
    public partial class ControllerBase : ComponentBase
    {

        /// <summary>
        /// Callback to trigger the display of error message on parent component
        /// </summary>
        [Parameter]
        public EventCallback<DialogMessage> OnShowErrorMessage { get; set; }

        /// <summary>
        /// Informs the view to display an error message captured by the controller.
        /// </summary>
        /// <param name="errorMessage">error message</param>
        protected virtual void RaiseShowErrorMessage(string errorMessage)
        {
            var info = new DialogMessage()
            {
                Title = "Internal Error Occurred",
                Message = errorMessage
            };
            OnShowErrorMessage.InvokeAsync(info);
        }

    }
}
