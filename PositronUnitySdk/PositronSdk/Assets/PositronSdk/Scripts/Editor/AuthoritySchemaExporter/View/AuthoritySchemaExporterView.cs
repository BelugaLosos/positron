using UnityEditor;

namespace Positron.Editor.AuthoritySchemaExporter
{
    public sealed class AuthoritySchemaExporterView
    {
        [MenuItem("Positron/Export schema")]
        public static void Export()
        {
            new AuthoritySchemaExporterPresenter().ExportSchemasToStreamingAssets();
        }
    }
}