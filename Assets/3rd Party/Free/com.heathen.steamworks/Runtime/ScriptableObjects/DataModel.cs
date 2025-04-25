#if !DISABLESTEAMWORKS  && STEAMWORKSNET
using Steamworks;
using System;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Abstract structure for game data models.
    /// </summary>
    /// <remarks>
    /// See <see cref="DataModel{T}"/> for more information on the usage of <see cref="DataModel"/>
    /// </remarks>
    [HelpURL("https://kb.heathen.group/assets/steamworks/data-models")]
    public abstract class DataModel : ScriptableObject
    {
        /// <summary>
        /// The extension associated with this model
        /// </summary>
        /// <remarks>
        /// When loading file addresses from Valve's backend the system will check if the address ends with this string.
        /// Note this test ignores case.
        /// When writing data to Valve's backend from this model the system will check for and append this extension if required
        /// </remarks>
        public string extension;

        [Header("Events")]
        public UnityEvent evtDataUpdated = new UnityEvent();

        [NonSerialized]
        public RemoteStorageFile[] availableFiles;

        /// <summary>
        /// Gets the base type of the data stored by this model
        /// </summary>
        public abstract Type DataType { get; }

        public void Refresh()
        {
            availableFiles = API.RemoteStorage.Client.GetFiles(extension);
        }

        public abstract void LoadByteArray(byte[] data);

        public abstract void LoadJson(string json);

        public abstract void LoadFileAddress(RemoteStorageFile address);

        public abstract void LoadFileAddress(string address);

        public abstract void LoadFileAddressAsync(RemoteStorageFile address, Action<bool> callback);

        public abstract void LoadFileAddressAsync(string address, Action<bool> callback);

        public abstract byte[] ToByteArray();

        public abstract string ToJson();

        public abstract bool Save(string filename);

        public abstract void SaveAsync(string filename, Action<RemoteStorageFileWriteAsyncComplete_t, bool> callback);
    }

    /// <summary>
    /// Used to create data models suitable for save and load operations via <see cref="RemoteStorageSystem"/>
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is an abstract class and must be inherited from to create a unique data model class suitable for your game.
    /// The use is similar to that of UnityEngine's UnityEvent&lt;T&gt; in that you can you create any data structure you like as a class or struct assuming of course that it is marked as [Serializable].
    /// You can then declare a class and derive from <see cref="DataModel{T}"/> as demonstrated below. 
    /// Note that <see cref="DataModel"/> is derived from Unity's ScriptableObject allowing you to create your data model object as an asset in your project
    /// </para>
    /// <code>
    /// [Serializable]
    /// public class MyCharacterData
    /// {
    ///     public string characterName;
    ///     public int level;
    ///     public Serializable.SerializableVector3 position;
    ///     public Serializable.SerializableQuaternion rotation;
    /// }
    ///
    /// [CreateAssetMenu(menuName = "My Objects/Character Data Model")]
    /// public class CharacterDataModel : DataModel&lt;MyCharacterData&gt; { }
    /// </code>
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public abstract class DataModel<T> : DataModel
    {
        /// <summary>
        /// The currently loaded data related to this model
        /// </summary>
        /// <remarks>
        /// The type provided must be serializable by Unity's JsonUtility
        /// </remarks>
        public T data;

        public override Type DataType => typeof(T);

        /// <summary>
        /// Stores <paramref name="data"/> to the <see cref="data"/> member
        /// </summary>
        /// <param name="data">The UTF8 encoded bytes of JSON representing this object</param>
        public override void LoadByteArray(byte[] data)
        {
            this.data = JsonUtility.FromJson<T>(Encoding.UTF8.GetString(data));
            evtDataUpdated.Invoke();
        }

        /// <summary>
        /// Stores the <paramref name="json"/> string to the <see cref="data"/> member
        /// </summary>
        /// <param name="json">The JSON formatted string containing the data of this object</param>
        public override void LoadJson(string json)
        {
            data = JsonUtility.FromJson<T>(json);
            evtDataUpdated.Invoke();
        }

        /// <summary>
        /// Returns a JSON formatted string representing the <see cref="data"/> member
        /// </summary>
        /// <returns>JSON formatted string of the <see cref="data"/> member</returns>
        public override string ToJson()
        {
            return JsonUtility.ToJson(data);
        }

        /// <summary>
        /// Returns the UTF8 encoded bytes of the JSON representation of the <see cref="data"/> member
        /// </summary>
        /// <returns></returns>
        public override byte[] ToByteArray()
        {
            return Encoding.UTF8.GetBytes(JsonUtility.ToJson(data));
        }

        /// <summary>
        /// Starts an asynchronous save operation for the <see cref="data"/> of this object
        /// </summary>
        /// <remarks>
        /// This will add <see cref="DataModel.extension"/> to the end of the name if required.
        /// </remarks>
        public override void SaveAsync(string filename, Action<RemoteStorageFileWriteAsyncComplete_t, bool> callback)
        {
            if (filename.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                API.RemoteStorage.Client.FileWriteAsync(filename, ToByteArray(), callback);
            else
                API.RemoteStorage.Client.FileWriteAsync(filename + extension, ToByteArray(), callback);
        }

        /// <summary>
        /// Saves the <see cref="data"/> member of this object with the name provided
        /// </summary>
        /// <remarks>
        /// This will add <see cref="DataModel.extension"/> to the end of the name if required.
        /// </remarks>
        /// <param name="filename"></param>
        public override bool Save(string filename)
        {
            if (filename.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                return API.RemoteStorage.Client.FileWrite(filename, ToByteArray());
            else
                return API.RemoteStorage.Client.FileWrite(filename + extension, ToByteArray());
        }

        /// <summary>
        /// Loads data from the address provided into <see cref="data"/>
        /// </summary>
        /// <param name="address">The address to load from</param>
        public override void LoadFileAddress(RemoteStorageFile address)
        {
            data = API.RemoteStorage.Client.FileReadJson<T>(address.name, Encoding.UTF8);
            evtDataUpdated?.Invoke();
        }

        /// <summary>
        /// Loads data from the address provided into <see cref="data"/>
        /// </summary>
        /// <param name="address">The address to load from</param>
        /// <param name="callback">An action to invoke when the process is complete, this can be null</param>
        public override void LoadFileAddressAsync(RemoteStorageFile address, Action<bool> callback)
        {
            API.RemoteStorage.Client.FileReadAsync(address.name, (r, e) =>
            {
                if (!e)
                {
                    var JsonString = System.Text.Encoding.UTF8.GetString(r);
                    data = JsonUtility.FromJson<T>(JsonString);
                    evtDataUpdated?.Invoke();
                }
                else
                    callback?.Invoke(!e);
            });
        }

        /// <summary>
        /// Loads data from the address provided into <see cref="data"/>
        /// </summary>
        /// <param name="address">The address to load from</param>
        public override void LoadFileAddress(string address)
        {
            data = API.RemoteStorage.Client.FileReadJson<T>(address, Encoding.UTF8);
            evtDataUpdated?.Invoke();
        }

        /// <summary>
        /// Loads data from the address provided into <see cref="data"/>
        /// </summary>
        /// <param name="address">The address to load from</param>
        /// <param name="callback">An action to invoke when the process is complete, this can be null</param>
        public override void LoadFileAddressAsync(string address, Action<bool> callback)
        {
            API.RemoteStorage.Client.FileReadAsync(address, (r,e) =>
            {
                if (!e)
                {
                    var JsonString = System.Text.Encoding.UTF8.GetString(r);
                    data = JsonUtility.FromJson<T>(JsonString);
                    evtDataUpdated?.Invoke();
                }
                else
                    callback?.Invoke(!e);
            });
        }
    }
}
#endif
