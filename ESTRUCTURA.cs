using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Linq;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using Microsoft.Identity.Client;



namespace PROYECTO_AGRI_AI
{
    internal class ESTRUCTURA
    {
        public static void GuardarEnBD(string prompt, string respuesta)
        {
            string connectionString = "Data Source=CRISTIAN\\SQLEXPRESS01;Initial Catalog=Proyecto_2;Integrated Security=True;TrustServerCertificate=True;";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO CONSULTA_AGRO (Prompt, Result) VALUES (@Prompt, @Result)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add("@Prompt", SqlDbType.NVarChar).Value = prompt;
                        command.Parameters.Add("@Result", SqlDbType.NVarChar).Value = respuesta;
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // Aquí se puede loggear un error o lanzar una excepción
                Console.WriteLine("Error al guardar en BD: " + ex.Message);
                throw;
            }
        }
        // funcion para crear la carpeta "archivosInvestigacion" en el escritorio
        public static void CrearCarpeta()
        {
            string archivosInvestigacion = @"C:\Users\Cccas\Desktop\ConsultasAgroAI-J";

            if (!Directory.Exists(archivosInvestigacion))
            {
                Directory.CreateDirectory(archivosInvestigacion);
                Console.WriteLine("Carpeta 'ConsultasAgroAI-J' creada correctamente!");
            }
            else
            {
                Console.WriteLine("La carpeta 'ConsultasAgroAI-J' ya existe!");
            }
        }
        //CREAR WORD Y GUARDAR EN CARPETA
        public static void CrearWord(string prompt, string respuesta)
        {
            string folderPath = @"C:\Users\Cccas\Desktop\ConsultasAgroAI-J";
            string fileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_ResearchResult.docx";
            string filePath = Path.Combine(folderPath, fileName);

            string logoUMGPath = @"C:\Users\Cccas\Desktop\LOGO_UMG.png";
            string elotePath = @"C:\Users\Cccas\Desktop\ELOTEX2.png";

            using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document))
            {
                MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new Document();
                Body body = new Body();

                void InsertarImagen(MainDocumentPart part, string imagePath, string imageId)
                {
                    ImagePart imagePart = part.AddImagePart(ImagePartType.Png, imageId);
                    using (FileStream stream = new FileStream(imagePath, FileMode.Open))
                    {
                        imagePart.FeedData(stream);
                    }
                }

                Drawing CrearImagen(string imageId, long anchoEmu, long altoEmu)
                {
                    return new Drawing(
                        new DocumentFormat.OpenXml.Drawing.Wordprocessing.Inline(
                            new DocumentFormat.OpenXml.Drawing.Wordprocessing.Extent() { Cx = anchoEmu, Cy = altoEmu },
                            new DocumentFormat.OpenXml.Drawing.Wordprocessing.EffectExtent()
                            {
                                LeftEdge = 0L,
                                TopEdge = 0L,
                                RightEdge = 0L,
                                BottomEdge = 0L
                            },
                            new DocumentFormat.OpenXml.Drawing.Wordprocessing.DocProperties() { Id = 1U, Name = "Imagen" },
                            new DocumentFormat.OpenXml.Drawing.Wordprocessing.NonVisualGraphicFrameDrawingProperties(
                            new DocumentFormat.OpenXml.Drawing.GraphicFrameLocks() { NoChangeAspect = true }),
                            new DocumentFormat.OpenXml.Drawing.Graphic(
                                new DocumentFormat.OpenXml.Drawing.GraphicData(
                                    new DocumentFormat.OpenXml.Drawing.Pictures.Picture(
                                        new DocumentFormat.OpenXml.Drawing.Pictures.NonVisualPictureProperties(
                                            new DocumentFormat.OpenXml.Drawing.Pictures.NonVisualDrawingProperties() { Id = 0U, Name = "Image" },
                                            new DocumentFormat.OpenXml.Drawing.Pictures.NonVisualPictureDrawingProperties()),
                                        new DocumentFormat.OpenXml.Drawing.Pictures.BlipFill(
                                            new DocumentFormat.OpenXml.Drawing.Blip()
                                            {
                                                Embed = imageId,
                                                CompressionState = DocumentFormat.OpenXml.Drawing.BlipCompressionValues.Print
                                            },
                                            new DocumentFormat.OpenXml.Drawing.Stretch(new DocumentFormat.OpenXml.Drawing.FillRectangle())),
                                        new DocumentFormat.OpenXml.Drawing.Pictures.ShapeProperties(
                                            new DocumentFormat.OpenXml.Drawing.Transform2D(
                                                new DocumentFormat.OpenXml.Drawing.Offset() { X = 0L, Y = 0L },
                                                new DocumentFormat.OpenXml.Drawing.Extents() { Cx = anchoEmu, Cy = altoEmu }),
                                            new DocumentFormat.OpenXml.Drawing.PresetGeometry(new DocumentFormat.OpenXml.Drawing.AdjustValueList())
                                            { Preset = DocumentFormat.OpenXml.Drawing.ShapeTypeValues.Rectangle })
                                    )
                                )
                                { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" }
                            )
                        )
                        { DistanceFromTop = 0U, DistanceFromBottom = 0U, DistanceFromLeft = 0U, DistanceFromRight = 0U }
                    );
                }

                // Insertamos imágenes
                InsertarImagen(mainPart, logoUMGPath, "imageUMG");
                InsertarImagen(mainPart, elotePath, "imageElote");

                
                // Imagen izquierda (UMG)
                Paragraph imgIzquierda = new Paragraph(
                    new ParagraphProperties(
                        new Justification() { Val = JustificationValues.Left },
                        new SpacingBetweenLines() { After = "200" } // Menor espacio
                    ),
                    new Run(CrearImagen("imageUMG", 1200000L, 1200000L)) // Tamaño reducido
                );

                // Imagen derecha (Elote)
                Paragraph imgDerecha = new Paragraph(
                    new ParagraphProperties(
                        new Justification() { Val = JustificationValues.Right },
                        new SpacingBetweenLines() { After = "200" } // Menor espacio
                    ),
                    new Run(CrearImagen("imageElote", 1200000L, 1200000L)) // Tamaño reducido
                );



                // Título centrado
                Paragraph titulo = new Paragraph(
                    new ParagraphProperties(
                        new Justification() { Val = JustificationValues.Center },
                        new SpacingBetweenLines() { After = "200" }
                    ),
                    new Run(
                        new RunProperties(
                            new Bold(),
                            new FontSize() { Val = "36" },
                            new Color() { Val = "2F5597" },
                            new RunFonts() { Ascii = "Calibri" }
                        ),
                        new Text("📘 Resultado de la AgroConsulta")
                    )
                );

                // Línea separadora
                Paragraph separador = new Paragraph(
                    new ParagraphProperties(new Justification() { Val = JustificationValues.Center }),
                    new Run(new RunProperties(new Color() { Val = "AAAAAA" }), new Text("────────────────────────────────────────────"))
                );

                // Prompt
                Paragraph promptTitulo = new Paragraph(
                    new ParagraphProperties(new SpacingBetweenLines() { After = "100" }),
                    new Run(new RunProperties(new Bold(), new FontSize() { Val = "28" }, new Color() { Val = "2E74B5" }), new Text("🗨️ :"))
                );

                Paragraph promptContenido = new Paragraph(
                    new Run(new RunProperties(new FontSize() { Val = "24" }), new Text(prompt))
                );

                // Respuesta
                Paragraph respuestaTitulo = new Paragraph(
                    new ParagraphProperties(new SpacingBetweenLines() { Before = "200", After = "100" }),
                    new Run(new RunProperties(new Bold(), new FontSize() { Val = "28" }, new Color() { Val = "70AD47" }), new Text("📎 Respuesta:"))
                );

                Paragraph respuestaContenido = new Paragraph(
                    new Run(new RunProperties(new FontSize() { Val = "24" }), new Text(respuesta))
                );

                // Bordes del documento
                SectionProperties sectionProps = new SectionProperties();
                PageBorders borders = new PageBorders()
                {
                    Display = PageBorderDisplayValues.AllPages,
                    ZOrder = PageBorderZOrderValues.Front
                };

                borders.Append(
                    new TopBorder() { Val = BorderValues.Double, Size = 24, Color = "4F81BD", Space = 20 },
                    new BottomBorder() { Val = BorderValues.Double, Size = 24, Color = "4F81BD", Space = 20 },
                    new LeftBorder() { Val = BorderValues.Double, Size = 24, Color = "4F81BD", Space = 20 },
                    new RightBorder() { Val = BorderValues.Double, Size = 24, Color = "4F81BD", Space = 20 }
                );

                sectionProps.Append(borders);

                // Armar documento
                body.Append(imgIzquierda, imgDerecha, titulo, separador, promptTitulo, promptContenido, respuestaTitulo, respuestaContenido, sectionProps);
                mainPart.Document.Append(body);
                mainPart.Document.Save();
            }
        }
    }
}


