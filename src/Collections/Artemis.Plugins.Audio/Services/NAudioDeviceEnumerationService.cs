﻿using System;
using System.Runtime.InteropServices;
using Artemis.Core.Services;
using Artemis.Plugins.LayerEffects.AudioVisualization.Interfaces;
using NAudio.CoreAudioApi;
using Serilog;

namespace Artemis.Plugins.LayerEffects.AudioVisualization.Services
{
    /// <summary>
    /// This service will act as a container for a single MMDeviceEnumerator for all plugin features. 
    /// to bypass a limitation of NAudio that don’t allow create more than one instance of the enumerator
    /// from different threads
    /// </summary>
    public class NAudioDeviceEnumerationService : IPluginService, IDisposable
    {
        #region Properties & Fields

        private readonly ILogger _logger;
        private MMDeviceEnumerator _deviceEnumerator;
        private INotificationClient _notificationClient;

        #endregion

        #region Constructors

        public NAudioDeviceEnumerationService(ILogger logger)
        {
            _logger = logger;

            _deviceEnumerator = new MMDeviceEnumerator();
            _logger.Information("Audio device enumerator service created.");

            _notificationClient = new INotificationClient();
            _deviceEnumerator.RegisterEndpointNotificationCallback(_notificationClient);
            _logger.Information("Audio device event interface registered.");
        }

        #endregion

        #region Methods

        /// <summary>
        /// Return the default INotificationClient used to register device changes event handlers. Return null if the Enumerator is not ready to be used
        /// </summary>
        public INotificationClient NotificationClient => _notificationClient;

        /// <summary>
        /// Get the current device for a given Flow direction and Device Role
        /// </summary>
        /// <param name="dataFlow">Audio data direction</param>
        /// <param name="role">Audio device role</param>
        /// <returns>Current device as MMDevice instance</returns>
        public MMDevice GetDefaultAudioEndpoint(DataFlow dataFlow, Role role)
        {
            try
            {
                return _deviceEnumerator.GetDefaultAudioEndpoint(dataFlow, role);
            }
            catch (AggregateException Ea)
            {
                _logger.Error("AggregateException on GetCurrentDevice() " + Ea);
            }
            catch (NullReferenceException Noe)
            {
                _logger.Error("NullReferenceException on GetCurrentDevice() " + Noe);
            }
            catch (Exception e)
            {
                _logger.Error("Another exception on GetCurrentDevice() " + e);
            }
            return null;
        }

        /// <summary>
        /// Get a collection of audio endpoints
        /// </summary>
        /// <param name="dataFlow">Audio data direction</param>
        /// <param name="swStateMask">Device state mask</param>
        /// <returns>Collection of audio endpoints</returns>
        public MMDeviceCollection EnumerateAudioEndPoints(DataFlow dataFlow, DeviceState swStateMask)
        {
            try
            {
                return _deviceEnumerator.EnumerateAudioEndPoints(dataFlow, swStateMask);
            }
            catch (AggregateException Ea)
            {
                _logger.Error("AggregateException on EnumerateAudioEndPoints() " + Ea);
            }
            catch (NullReferenceException Noe)
            {
                _logger.Error("NullReferenceException on EnumerateAudioEndPoints() " + Noe);
            }
            catch (Exception e)
            {
                _logger.Error("Another exception on EnumerateAudioEndPoints() " + e);
            }
            return null;
        }

        public void Dispose()
        {
            _deviceEnumerator.UnregisterEndpointNotificationCallback(_notificationClient);
            _logger.Information("Audio device event interface unregistered.");
            _deviceEnumerator?.Dispose();
            _deviceEnumerator = null;
            _logger.Information("Audio device enumerator service disposed.");
        }

        #endregion

    }
}