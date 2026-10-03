using Positron.Client.Settings;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Positron.Editor.AuthoritySchemaExporter
{
    public sealed class AuthoritySchemaExporterPresenter
    {
        private readonly AuthoritySchemaExporterModel _model;

        private const string EXPORTED_FILE_NAME = "AuthoritySchema.json";

        public AuthoritySchemaExporterPresenter()
        {
            _model = new();
        }

        public void ExportSchemasToStreamingAssets()
        {
            PositronSettings settings = Resources.Load<PositronSettings>(PositronSettings.RESOURCES_PATH);
            string json = JsonUtility.ToJson(_model.ExportObject(settings.SpawnableObjects), true);
            string path = Path.Combine(Application.streamingAssetsPath, EXPORTED_FILE_NAME);

            if (!Directory.Exists(Application.streamingAssetsPath))
            {
                Directory.CreateDirectory(Application.streamingAssetsPath);
            }

            File.WriteAllText(path, json);
            AssetDatabase.Refresh();
        }
    }
}