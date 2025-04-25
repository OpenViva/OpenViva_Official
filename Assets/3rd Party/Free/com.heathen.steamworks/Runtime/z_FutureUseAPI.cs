#if !DISABLESTEAMWORKS && STEAMWORKSNET
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Heathen.SteamworksIntegration.API
{
#region Future Use
    /*
    /// <summary>
    /// Interface for rendering and interacting with HTML pages.
    /// </summary>
    /// <remarks>
    /// You can use this interface to render and display HTML pages directly inside your game or application. You must call Init prior to using this interface, and Shutdown when you're done using it.
    /// </remarks>
    public static class HTMLSurface
    {
        public static class Client
        {
            /// <summary>
            /// Called when a browser wants to navigate to a new page.
            /// </summary>
            /// <remarks>
            /// You must call <see cref="AllowStartRequest(HHTMLBrowser, bool)"/> in responce to this;
            /// </remarks>
            public static HTML_StartRequestEvent EventHTML_StartRequest
            {
                get
                {
                    if (m_HTML_StartRequest_t == null)
                        m_HTML_StartRequest_t = Callback<HTML_StartRequest_t>.Create(eventHTML_StartRequest.Invoke);

                    return eventHTML_StartRequest;
                }
            }
            /// <summary>
            /// Called when the browser wants to display a Javascript alert dialog, call JSDialogResponse when the user dismisses this dialog; or right away to ignore it.
            /// </summary>
            public static HTML_JSAlertEvent EventHTML_JSAlert
            {
                get
                {
                    if (m_HTML_JSAlert_t == null)
                        m_HTML_JSAlert_t = Callback<HTML_JSAlert_t>.Create(eventHTML_JSAlert.Invoke);

                    return eventHTML_JSAlert;
                }
            }
            /// <summary>
            /// Called when the browser wants to display a Javascript confirmation dialog, call JSDialogResponse when the user dismisses this dialog; or right away to ignore it.
            /// </summary>
            public static HTML_JSConfirmEvent EventHTML_JSConfirm
            {
                get
                {
                    if (HTML_JSConfirm_t == null)
                        HTML_JSConfirm_t = Callback<HTML_JSConfirm_t>.Create(eventHTML_JSConfirm.Invoke);

                    return eventHTML_JSConfirm;
                }
            }
            /// <summary>
            /// Called when a browser surface has received a file open dialog from a <input type="file"> click or similar, you must call FileLoadDialogResponse with the file(s) the user selected.
            /// </summary>
            public static HTML_FileOpenDialogEvent EventHTML_FileOpenDialog
            {
                get
                {
                    if (HTML_FileOpenDialog_t == null)
                        HTML_FileOpenDialog_t = Callback<HTML_FileOpenDialog_t>.Create(eventHTML_FileOpenDialog.Invoke);

                    return eventHTML_FileOpenDialog;
                }
            }

            private static HTML_StartRequestEvent eventHTML_StartRequest = new();
            private static HTML_JSAlertEvent eventHTML_JSAlert = new();
            private static HTML_JSConfirmEvent eventHTML_JSConfirm = new();
            private static HTML_FileOpenDialogEvent eventHTML_FileOpenDialog = new();

            private static Callback<HTML_StartRequest_t> m_HTML_StartRequest_t;
            private static Callback<HTML_JSAlert_t> m_HTML_JSAlert_t;
            private static Callback<HTML_JSConfirm_t> HTML_JSConfirm_t;
            private static Callback<HTML_FileOpenDialog_t> HTML_FileOpenDialog_t;

            private static CallResult<HTML_BrowserReady_t> m_HTML_BrowserReady_t;

            /// <summary>
            /// Add a header to any HTTP requests from this browser.
            /// </summary>
            /// <param name="browser"></param>
            /// <param name="key"></param>
            /// <param name="value"></param>
            public static void AddHeader(HHTMLBrowser browser, string key, string value) => SteamHTMLSurface.AddHeader(browser, key, value);
            /// <summary>
            /// Sets whether a pending load is allowed or if it should be canceled.
            /// </summary>
            /// <remarks>
            /// NOTE:You MUST call this in response to a <see cref="EventHTML_StartRequest"/> callback.
            /// </remarks>
            /// <param name="browser"></param>
            /// <param name="allowed"></param>
            public static void AllowStartRequest(HHTMLBrowser browser, bool allowed) => SteamHTMLSurface.AllowStartRequest(browser, allowed);
            /// <summary>
            /// Copy the currently selected text from the current page in an HTML surface into the local clipboard.
            /// </summary>
            /// <param name="browser"></param>
            public static void CopyToClipboard(HHTMLBrowser browser) => SteamHTMLSurface.CopyToClipboard(browser);
            /// <summary>
            /// Create a browser object for displaying of an HTML page.
            /// </summary>
            /// <param name="agent">Appends the string to the general user agent string of the browser, allowing you to detect your client on webservers. Use NULL if you do not require this functionality.</param>
            /// <param name="CSSStyle">This allows you to set a CSS style to every page displayed by this browser. Use NULL if you do not require this functionality.</param>
            /// <param name="callback">Invoked when the browser is creatd and contains the required <see cref="HHTMLBrowser"/> handle used in other calls</param>
            /// <remarks>
            /// <para>You must handle <see cref="EventHTML_JSAlert"/>, <see cref="EventHTML_JSConfirm"/> and <see cref="EventHTML_FileOpenDialog"/>!</para>
            /// <para>If you do not implement these handlers, the browser may appear to hang instead of navigating to new pages or triggering javascript popups!</para>
            /// <para>You MUST call <see cref="RemoveBrowser"/> when you are done using this browser to free up the resources associated with it. Failing to do so will result in a memory leak.</para>
            /// </remarks>
            public static void CreateBrowser(string agent, string CSSStyle, Action<HTML_BrowserReady_t, bool> callback)
            {
                if (callback == null)
                    return;

                if (m_HTML_BrowserReady_t == null)
                    m_HTML_BrowserReady_t = CallResult<HTML_BrowserReady_t>.Create();

                var handle = SteamHTMLSurface.CreateBrowser(agent, CSSStyle);
                m_HTML_BrowserReady_t.Set(handle, callback.Invoke);
            }
            /// <summary>
            /// Run a javascript script in the currently loaded page.
            /// </summary>
            /// <param name="browser"></param>
            /// <param name="script"></param>
            public static void ExecuteJavascript(HHTMLBrowser browser, string script) => SteamHTMLSurface.ExecuteJavascript(browser, script);
            /// <summary>
            /// This is not martialled properly in Steamworks.NET so cant be used without additional work managing your own IntPrt
            /// </summary>
            /// <param name="browser"></param>
            /// <param name="selectedFiles"></param>
            public static void FileLoadDialogResponse(HHTMLBrowser browser, IntPtr selectedFiles) => SteamHTMLSurface.FileLoadDialogResponse(browser, selectedFiles);
            /// <summary>
            /// Find a string in the current page of an HTML surface.
            /// </summary>
            /// <remarks>
            /// This is the equivalent of "ctrl+f" in your browser of choice. It will highlight all of the matching strings.
            /// You should call StopFind when the input string has changed or you want to stop searching.
            /// </remarks>
            /// <param name="browser"></param>
            /// <param name="searchString"></param>
            /// <param name="currentlyInFind"></param>
            /// <param name="reverse"></param>
            public static void Find(HHTMLBrowser browser, string searchString, bool currentlyInFind, bool reverse) => SteamHTMLSurface.Find(browser, searchString, currentlyInFind, reverse);
            /// <summary>
            /// Retrieves details about a link at a specific position on the current page in an HTML surface.
            /// </summary>
            /// <param name="browser"></param>
            /// <param name="x"></param>
            /// <param name="y"></param>
            public static void GetLinkAtPosition(HHTMLBrowser browser, int x, int y) => SteamHTMLSurface.GetLinkAtPosition(browser, x, y);
            /// <summary>
            /// Navigate back in the page history.
            /// </summary>
            /// <param name="browser"></param>
            public static void GoBack(HHTMLBrowser browser) => SteamHTMLSurface.GoBack(browser);
            /// <summary>
            /// Initializes the HTML Surface API.
            /// </summary>
            /// <remarks>
            /// <para>
            /// This must be called prior to using any other functions in this interface.
            /// </para>
            /// <para>
            /// You MUST call Shutdown when you are done using the interface to free up the resources associated with it. Failing to do so will result in a memory leak!
            /// </para>
            /// </remarks>
            /// <returns>true if the API was successfully initialized; otherwise, false.</returns>
            public static bool Init() => SteamHTMLSurface.Init();
            /// <summary>
            /// Allows you to react to a page wanting to open a javascript modal dialog notification.
            /// </summary>
            /// <param name="browser">The handle of the surface that is spawning a dialog.</param>
            /// <param name="result">Set this to true to simulate pressing the "OK" button, otherwise false for "Cancel".</param>
            public static void JSDialogResponse(HHTMLBrowser browser, bool result) => SteamHTMLSurface.JSDialogResponse(browser, result);
            /// <summary>
            /// cUnicodeChar is the unicode character point for this keypress (and potentially multiple chars per press)
            /// </summary>
            /// <param name="browser">The handle of the surface to send the interaction to.</param>
            /// <param name="unicodeChar">The unicode character point for this keypress; and potentially multiple characters per press.</param>
            /// <param name="eHTMLKeyModifiers">This should be set to a bitmask of the modifier keys that the user is currently pressing.</param>
            public static void KeyChar(HHTMLBrowser browser, uint unicodeChar, EHTMLKeyModifiers eHTMLKeyModifiers) => SteamHTMLSurface.KeyChar(browser, unicodeChar, eHTMLKeyModifiers);
            /// <summary>
            /// keyboard interactions, native keycode is the virtual key code value from your OS
            /// </summary>
            /// <param name="browser">The handle of the surface to send the interaction to.</param>
            /// <param name="nNativeKeyCode">This is the virtual keycode value from the OS.</param>
            /// <param name="eHTMLKeyModifiers">This should be set to a bitmask of the modifier keys that the user is currently pressing.</param>
            public static void KeyDown(HHTMLBrowser browser, uint nNativeKeyCode, EHTMLKeyModifiers eHTMLKeyModifiers) => SteamHTMLSurface.KeyDown(browser, nNativeKeyCode, eHTMLKeyModifiers);
            /// <summary>
            /// 
            /// </summary>
            /// <param name="browser">The handle of the surface to send the interaction to.</param>
            /// <param name="nNativeKeyCode">This is the virtual keycode value from the OS.</param>
            /// <param name="eHTMLKeyModifiers">This should be set to a bitmask of the modifier keys that the user is currently pressing.</param>
            public static void KeyUp(HHTMLBrowser browser, uint nNativeKeyCode, EHTMLKeyModifiers eHTMLKeyModifiers) => SteamHTMLSurface.KeyUp(browser, nNativeKeyCode, eHTMLKeyModifiers);
            /// <summary>
            /// Navigate to a specified URL.
            /// </summary>
            /// <remarks>
            /// You can load any URI scheme supported by Chromium Embedded Framework including but not limited to: http://, https://, ftp://, and file:///. If no scheme is specified then http:// is used.
            /// </remarks>
            /// <param name="browser"></param>
            /// <param name="url"></param>
            /// <param name="postData">If you send POST data with pchPostData then the data should be formatted as: name1=value1&name2=value2.</param>
            public static void LoadURL(HHTMLBrowser browser, string url, string postData) => SteamHTMLSurface.LoadURL(browser, url, postData);
            /// <summary>
            /// Tells an HTML surface that a mouse button has been double clicked.
            /// </summary>
            /// <param name="browser">The handle of the surface to send the interaction to.</param>
            /// <param name="eMouseButton">The mouse button which was double clicked.</param>
            public static void MouseDoubleClick(HHTMLBrowser browser, EHTMLMouseButton eMouseButton) => SteamHTMLSurface.MouseDoubleClick(browser, eMouseButton);
            /// <summary>
            /// Tells an HTML surface where the mouse is.
            /// </summary>
            /// <param name="browser">The handle of the surface to send the interaction to.</param>
            /// <param name="x">X (width) coordinate in pixels relative to the position of the HTML surface. (0, 0) is the top left.</param>
            /// <param name="y">Y (height) coordinate in pixels relative to the position of the HTML surface. (0, 0) is the top left.</param>
            public static void MouseMove(HHTMLBrowser browser, int x, int y) => SteamHTMLSurface.MouseMove(browser, x, y);
            /// <summary>
            /// Tells an HTML surface that a mouse button has been released.
            /// </summary>
            /// <remarks>
            /// The click will occur where the surface thinks the mouse is based on the last call to MouseMove.
            /// </remarks>
            /// <param name="browser"></param>
            /// <param name="eMouseButton"></param>
            public static void MouseUp(HHTMLBrowser browser, EHTMLMouseButton eMouseButton) => SteamHTMLSurface.MouseUp(browser, eMouseButton);
            /// <summary>
            /// Tells an HTML surface that the mouse wheel has moved.
            /// </summary>
            /// <param name="browser">The handle of the surface to send the interaction to.</param>
            /// <param name="delta">The number of pixels to scroll.</param>
            public static void MouseWheel(HHTMLBrowser browser, int delta) => SteamHTMLSurface.MouseWheel(browser, delta);
            public static void OpenDeveloperTools(HHTMLBrowser browser) => SteamHTMLSurface.OpenDeveloperTools(browser);
            /// <summary>
            /// Paste from the local clipboard to the current page in an HTML surface.
            /// </summary>
            /// <param name="browser"></param>
            public static void PasteFromClipboard(HHTMLBrowser browser) => SteamHTMLSurface.PasteFromClipboard(browser);
            /// <summary>
            /// Refreshes the current page
            /// </summary>
            /// <remarks>
            /// The reload will most likely hit the local cache instead of going over the network. This is equivalent to F5 or Ctrl+R in your browser of choice.
            /// </remarks>
            /// <param name="browser"></param>
            public static void Reload(HHTMLBrowser browser) => SteamHTMLSurface.Reload(browser);
            /// <summary>
            /// You MUST call this when you are done with an HTML surface, freeing the resources associated with it.
            /// Failing to call this will result in a memory leak!
            /// </summary>
            /// <param name="browser"></param>
            public static void RemoveBrowser(HHTMLBrowser browser) => SteamHTMLSurface.RemoveBrowser(browser);
            /// <summary>
            /// Enable/disable low-resource background mode, where javascript and repaint timers are throttled, resources are more aggressively purged from memory, and audio/video elements are paused.
            /// </summary>
            /// <param name="browser"></param>
            /// <param name="backgroundMode"></param>
            public static void SetBackgroundMode(HHTMLBrowser browser, bool backgroundMode) => SteamHTMLSurface.SetBackgroundMode(browser, backgroundMode);
            /// <summary>
            /// Set a webcookie for a specific hostname.
            /// </summary>
            /// <param name="hostName">The hostname of the server to set the cookie for. ('Host' attribute)</param>
            /// <param name="key">The cookie name to set.</param>
            /// <param name="value">The cookie value to set.</param>
            /// <param name="path">Sets the 'Path' attribute on the cookie. You can use this to restrict the cookie to a specific path on the domain. e.g. "/accounts"</param>
            /// <param name="expires">Sets the 'Expires' attribute on the cookie to the specified timestamp in Unix epoch format (seconds since Jan 1st 1970).</param>
            /// <param name="secure">Sets the 'Secure' attribute.</param>
            /// <param name="httpOnly">Sets the 'HttpOnly' attribute.</param>
            public static void SetCookie(string hostName, string key, string value, string path = "/", uint expires = 0, bool secure = false, bool httpOnly = false) => SteamHTMLSurface.SetCookie(hostName, key, value, path, expires, secure, httpOnly);
            /// <summary>
            /// Scroll the current page horizontally.
            /// </summary>
            /// <param name="browser"></param>
            /// <param name="absolutePixelScroll"></param>
            public static void SetHorizonntalScroll(HHTMLBrowser browser, uint absolutePixelScroll) => SteamHTMLSurface.SetHorizontalScroll(browser, absolutePixelScroll);
            /// <summary>
            /// Tell a HTML surface if it has key focus currently, controls showing the I-beam cursor in text controls amongst other things.
            /// </summary>
            /// <param name="browser"></param>
            /// <param name="hasKeyFocus"></param>
            public static void SetKeyFocus(HHTMLBrowser browser, bool hasKeyFocus) => SteamHTMLSurface.SetKeyFocus(browser, hasKeyFocus);
            /// <summary>
            /// Zoom the current page in an HTML surface.
            /// </summary>
            /// <param name="browser"></param>
            /// <param name="zoom"></param>
            /// <param name="x"></param>
            /// <param name="y"></param>
            public static void SetPageScaleFactor(HHTMLBrowser browser, float zoom, int x, int y) => SteamHTMLSurface.SetPageScaleFactor(browser, zoom, x, y);
            /// <summary>
            /// Sets the display size of a surface in pixels.
            /// </summary>
            /// <param name="browser"></param>
            /// <param name="width"></param>
            /// <param name="height"></param>
            public static void SetSize(HHTMLBrowser browser, uint width, uint height) => SteamHTMLSurface.SetSize(browser, width, height);
            /// <summary>
            /// Scroll the current page vertically.
            /// </summary>
            /// <param name="browser"></param>
            /// <param name="absolutePixelScroll"></param>
            public static void SetVerticalScroll(HHTMLBrowser browser, uint absolutePixelScroll) => SteamHTMLSurface.SetVerticalScroll(browser, absolutePixelScroll);
        }
    }
    public static class HTTP
    {
        public static class Client
        { }
    }
    public static class Music
    {
        public static class Client
        { }
    }
    public static class Video
    {
        public static class Client
        { }
    }
    //*/
#endregion
}
#endif