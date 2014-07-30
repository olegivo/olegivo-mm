﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Oleg_ivo.MeloManager.ServiceReference1 {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="ServiceReference1.IWinampService", CallbackContract=typeof(Oleg_ivo.MeloManager.ServiceReference1.IWinampServiceCallback))]
    public interface IWinampService {
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IWinampService/LoadPlaylist")]
        void LoadPlaylist(string playlistFilename);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IWinampService/LoadPlaylist")]
        System.Threading.Tasks.Task LoadPlaylistAsync(string playlistFilename);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IWinampService/Ping")]
        void Ping();
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IWinampService/Ping")]
        System.Threading.Tasks.Task PingAsync();
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IWinampServiceCallback {
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IWinampService/OnCurrentSongChanged")]
        void OnCurrentSongChanged(string filename);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IWinampServiceChannel : Oleg_ivo.MeloManager.ServiceReference1.IWinampService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class WinampServiceClient : System.ServiceModel.DuplexClientBase<Oleg_ivo.MeloManager.ServiceReference1.IWinampService>, Oleg_ivo.MeloManager.ServiceReference1.IWinampService {
        
        public WinampServiceClient(System.ServiceModel.InstanceContext callbackInstance) : 
                base(callbackInstance) {
        }
        
        public WinampServiceClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName) : 
                base(callbackInstance, endpointConfigurationName) {
        }
        
        public WinampServiceClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, string remoteAddress) : 
                base(callbackInstance, endpointConfigurationName, remoteAddress) {
        }
        
        public WinampServiceClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(callbackInstance, endpointConfigurationName, remoteAddress) {
        }
        
        public WinampServiceClient(System.ServiceModel.InstanceContext callbackInstance, System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(callbackInstance, binding, remoteAddress) {
        }
        
        public void LoadPlaylist(string playlistFilename) {
            base.Channel.LoadPlaylist(playlistFilename);
        }
        
        public System.Threading.Tasks.Task LoadPlaylistAsync(string playlistFilename) {
            return base.Channel.LoadPlaylistAsync(playlistFilename);
        }
        
        public void Ping() {
            base.Channel.Ping();
        }
        
        public System.Threading.Tasks.Task PingAsync() {
            return base.Channel.PingAsync();
        }
    }
}
