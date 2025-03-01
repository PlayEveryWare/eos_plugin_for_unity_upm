/*
 * Copyright (c) 2021 PlayEveryWare
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

#if !EOS_DISABLE
#if !UNITY_EDITOR && !EXTERNAL_TO_UNITY
using UnityEngine.Scripting;
[assembly: AlwaysLinkAssembly]
#endif
namespace PlayEveryWare.EpicOnlineServices
{
    using System;
    using System.Collections.Generic;

    // The EXTERNAL_TO_UNITY block is here to enable the compilation of this
    // code file outside of the context of Unity altogether.
#if !EXTERNAL_TO_UNITY
    using UnityEngine;
#endif

    using JsonUtility = Utility.JsonUtility;

    public abstract class PlatformSpecifics<T> : IPlatformSpecifics where T : PlatformConfig
    {
        protected PlatformManager.Platform Platform;

        #region Methods for which the functionality is shared (consider these "sealed")

        protected PlatformSpecifics(PlatformManager.Platform platform)
        {
            Platform = platform;
        }

        public string GetDynamicLibraryExtension()
        {
            return PlatformManager.GetDynamicLibraryExtension(Platform);
        }

        #endregion

        #region Virtual methods that have a default behavior, but may need to be overwritten by deriving classes.

        public virtual string GetTempDir()
        {
            return Application.temporaryCachePath;
        }

        // The EXTERNAL_TO_UNITY block is here to enable the compilation of this
        // code file outside of the context of Unity altogether.
#if !EXTERNAL_TO_UNITY
        public virtual void InitializeOverlay(IEOSCoroutineOwner owner)
        {
            // default behavior is to take no action.
        }
#endif

        public virtual void AddPluginSearchPaths(ref List<string> pluginPaths)
        {
            // default behavior is to take no action.
        }

        public virtual void LoadDelegatesWithEOSBindingAPI()
        {
            // default behavior is to take no action.
        }

        public virtual void RegisterForPlatformNotifications()
        {
            // default behavior is to take no action.
        }

        public virtual void SetDefaultAudioSession()
        {
            // default behavior is to take no action.
        }

        public virtual bool IsApplicationConstrainedWhenOutOfFocus()
        {
            // by default applications are not considered constrained when out of focus
            // this might be different on future platforms.
            return false;
        }

        public virtual void ConfigureSystemPlatformCreateOptions(ref EOSCreateOptions createOptions)
        {
            ((EOSCreateOptions)createOptions).options.RTCOptions = new();
        }

        // The EXTERNAL_TO_UNITY block is here to enable the compilation of this
        // code file outside of the context of Unity altogether.
#if !EXTERNAL_TO_UNITY
        public virtual void ConfigureSystemInitOptions(ref EOSInitializeOptions initializeOptionsRef)
        {
            // Default implementation is to do nothing
        }
#endif
        /// <summary>
        /// Indicates whether the platform is ready for network activity.
        /// TODO: Determine where this is used, and why it isn't a boolean.
        /// </summary>
        /// <returns>1 if network is ready, 0 otherwise</returns>
        public virtual Int32 IsReadyForNetworkActivity()
        {
            // Default behavior is to assume that the platform is always ready for network activity.
            return 1;
        }

        public virtual void UpdateNetworkStatus()
        {
        }

        #endregion
    }
}
#endif //!EOS_DISABLE