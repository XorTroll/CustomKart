using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using MKDS_Course_Modifier.Converters;
using MKDS_Course_Modifier.Converters._3D;
using MKDS_Course_Modifier.G3D_Binary_File_Format;
using MKDS_Course_Modifier._3D_Formats;
using MKDS_Course_Modifier.Sound;
using OpenTK;
using Tao.OpenGl;

namespace CustomKart.Extras
{
    public static class MKDSCMExtras
    {
		private static float HelpersMax(params float[] v)
		{
			float num = float.MinValue;
			foreach (float num2 in v)
			{
				if (num2 > num)
				{
					num = num2;
				}
			}
			return num;
		}

		public static OBJ FixNitroUV(OBJ Input)
		{
			var mLT = new MLT(Input.MLTName);
            var oBJ = new OBJ
            {
                MLTName = Input.MLTName,
                Vertices = Input.Vertices,
                Normals = Input.Normals
            };
            int num = 0;
			foreach (var face2 in Input.Faces)
			{
				Vector2[] array = new Vector2[3]
				{
					Input.TexCoords[face2.TexCoordIndieces[0]],
					Input.TexCoords[face2.TexCoordIndieces[1]],
					Input.TexCoords[face2.TexCoordIndieces[2]]
				};
				MLT.Material materialByName = mLT.GetMaterialByName(face2.MaterialName);
				if (materialByName.DiffuseMap != null)
				{
					float num2 = 2047.9375f / (float)materialByName.DiffuseMap.Width;
					float num3 = -2048f / (float)materialByName.DiffuseMap.Width;
					float num4 = 2047.9375f / (float)materialByName.DiffuseMap.Height;
					float num5 = -2048f / (float)materialByName.DiffuseMap.Height;
					float num6 = array[0].X % 1f;
					float num7 = array[0].Y % 1f;
					array[1].X = array[1].X - array[0].X + num6;
					array[1].Y = array[1].Y - array[0].Y + num7;
					array[2].X = array[2].X - array[0].X + num6;
					array[2].Y = array[2].Y - array[0].Y + num7;
					array[0].X = num6;
					array[0].Y = num7;
					while (array[0].X <= num3 || array[1].X <= num3 || array[2].X <= num3)
					{
						array[0].X += 1f;
						array[1].X += 1f;
						array[2].X += 1f;
					}
					while (array[0].X >= num2 || array[1].X >= num2 || array[2].X >= num2)
					{
						array[0].X -= 1f;
						array[1].X -= 1f;
						array[2].X -= 1f;
					}
					while (array[0].Y <= num5 || array[1].Y <= num5 || array[2].Y <= num5)
					{
						array[0].Y += 1f;
						array[1].Y += 1f;
						array[2].Y += 1f;
					}
					while (array[0].Y >= num4 || array[1].Y >= num4 || array[2].Y >= num4)
					{
						array[0].Y -= 1f;
						array[1].Y -= 1f;
						array[2].Y -= 1f;
					}
					if (array[0].X <= num3 || array[1].X <= num3 || array[2].X <= num3 || array[0].X >= num2 || array[1].X >= num2 || array[2].X >= num2 || array[0].Y <= num5 || array[1].Y <= num5 || array[2].Y <= num5 || array[0].Y >= num4 || array[1].Y >= num4 || array[2].Y >= num4)
					{
						// MessageBox.Show("Your model seems to contain a face which texture is repeated too many to fix. Try splitting the face up, or less repeating the texture before trying again.\r\nMaterial Name: " + face2.MaterialName + "\r\nThere may be more though.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
						return null;
					}
				}
				oBJ.TexCoords.AddRange(array);
				var face = new OBJ.Face();
				face.MaterialName = face2.MaterialName;
				face.NormalIndieces = face2.NormalIndieces;
				face.TexCoordIndieces.AddRange(new int[3]
				{
					num,
					num + 1,
					num + 2
				});
				num += 3;
				face.VertexIndieces = face2.VertexIndieces;
				oBJ.Faces.Add(face);
			}
			return oBJ;
		}

		public static void GenerateNSBMD(string obj_path, float scale, bool has_texture, out byte[] nsbmd, out byte[] nsbtx)
        {
			var oBJ = FixNitroUV(new OBJ(obj_path));
			if (oBJ == null)
			{
				throw new Exception();
			}
			MLT mLT = new MLT(oBJ.MLTName);
			List<string> list = new List<string>();
			Vector3 right = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
			Vector3 left = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
			int i;
			for (i = 0; i < oBJ.Vertices.Count; i++)
			{
				oBJ.Vertices[i] = Vector3.Multiply(oBJ.Vertices[i], scale);
				if (oBJ.Vertices[i].X < right.X)
				{
					right.X = oBJ.Vertices[i].X;
				}
				if (oBJ.Vertices[i].X > left.X)
				{
					left.X = oBJ.Vertices[i].X;
				}
				if (oBJ.Vertices[i].Y < right.Y)
				{
					right.Y = oBJ.Vertices[i].Y;
				}
				if (oBJ.Vertices[i].Y > left.Y)
				{
					left.Y = oBJ.Vertices[i].Y;
				}
				if (oBJ.Vertices[i].Z < right.Z)
				{
					right.Z = oBJ.Vertices[i].Z;
				}
				if (oBJ.Vertices[i].Z > left.Z)
				{
					left.Z = oBJ.Vertices[i].Z;
				}
			}
			Vector3 vector = left - right;
			float num = HelpersMax(vector.X, vector.Y, vector.Z);
			float num2 = 1f;
			int num3 = 0;
			while (num > 7.999756f)
			{
				num3++;
				num2 /= 2f;
				num /= 2f;
			}
			for (i = 0; i < oBJ.Vertices.Count; i++)
			{
				oBJ.Vertices[i] = Vector3.Multiply(oBJ.Vertices[i], num2);
			}
			NSBMD nSBMD = new NSBMD(!has_texture);
			nSBMD.modelSet = new NSBMD.ModelSet();
			string text = Path.GetFileNameWithoutExtension(obj_path);
			if (text.Length > 16)
			{
				text = text.Remove(16);
			}
			nSBMD.modelSet.dict = new Dictionary<NSBMD.ModelSet.MDL0Data>();
			nSBMD.modelSet.dict.Add(text, new NSBMD.ModelSet.MDL0Data());
			nSBMD.modelSet.models = new NSBMD.ModelSet.Model[1];
			nSBMD.modelSet.models[0] = new NSBMD.ModelSet.Model();
			nSBMD.modelSet.models[0].info = new NSBMD.ModelSet.Model.ModelInfo();
			nSBMD.modelSet.models[0].info.numNode = 1;
			foreach (OBJ.Face face in oBJ.Faces)
			{
				if (!list.Contains(face.MaterialName))
				{
					list.Add(face.MaterialName);
				}
				nSBMD.modelSet.models[0].info.numTriangle++;
			}
			nSBMD.modelSet.models[0].info.numMat = (byte)list.Count;
			nSBMD.modelSet.models[0].info.numShp = (byte)list.Count;
			nSBMD.modelSet.models[0].info.firstUnusedMtxStackID = 1;
			nSBMD.modelSet.models[0].info.posScale = 1 << num3;
			nSBMD.modelSet.models[0].info.invPosScale = 1f / nSBMD.modelSet.models[0].info.posScale;
			nSBMD.modelSet.models[0].info.numVertex = (byte)oBJ.Vertices.Count;
			nSBMD.modelSet.models[0].info.numPolygon = (byte)oBJ.Faces.Count;
			nSBMD.modelSet.models[0].info.boxX = right.X * num2;
			nSBMD.modelSet.models[0].info.boxY = right.Y * num2;
			nSBMD.modelSet.models[0].info.boxZ = right.Z * num2;
			nSBMD.modelSet.models[0].info.boxW = vector.X * num2;
			nSBMD.modelSet.models[0].info.boxH = vector.Y * num2;
			nSBMD.modelSet.models[0].info.boxD = vector.Z * num2;
			nSBMD.modelSet.models[0].info.boxPosScale = 1 << num3;
			nSBMD.modelSet.models[0].info.boxInvPosScale = 1f / nSBMD.modelSet.models[0].info.boxPosScale;
			nSBMD.modelSet.models[0].nodes = new NSBMD.ModelSet.Model.NodeSet();
			nSBMD.modelSet.models[0].nodes.dict = new Dictionary<NSBMD.ModelSet.Model.NodeSet.NodeSetData>();
			nSBMD.modelSet.models[0].nodes.dict.Add("world_root", new NSBMD.ModelSet.Model.NodeSet.NodeSetData());
			nSBMD.modelSet.models[0].nodes.data = new NSBMD.ModelSet.Model.NodeSet.NodeData[1];
			nSBMD.modelSet.models[0].nodes.data[0] = new NSBMD.ModelSet.Model.NodeSet.NodeData();
			nSBMD.modelSet.models[0].nodes.data[0].flag = 7;
			nSBMD.modelSet.models[0].materials = new NSBMD.ModelSet.Model.MaterialSet();
			nSBMD.modelSet.models[0].materials.dictTexToMatList = new Dictionary<NSBMD.ModelSet.Model.MaterialSet.TexToMatData>();
			nSBMD.modelSet.models[0].materials.dictPlttToMatList = new Dictionary<NSBMD.ModelSet.Model.MaterialSet.PlttToMatData>();
			nSBMD.modelSet.models[0].materials.dict = new Dictionary<NSBMD.ModelSet.Model.MaterialSet.MaterialSetData>();
			nSBMD.modelSet.models[0].materials.materials = new NSBMD.ModelSet.Model.MaterialSet.Material[list.Count];
			i = 0;
			int num4 = 0;
			foreach (string item in list)
			{
				MLT.Material materialByName = mLT.GetMaterialByName(item);
				if (materialByName.DiffuseMap != null)
				{
					nSBMD.modelSet.models[0].materials.dictTexToMatList.Add(i.ToString() + "_t", new NSBMD.ModelSet.Model.MaterialSet.TexToMatData());
					nSBMD.modelSet.models[0].materials.dictPlttToMatList.Add(i.ToString() + "_p", new NSBMD.ModelSet.Model.MaterialSet.PlttToMatData());
					nSBMD.modelSet.models[0].materials.dictTexToMatList[num4].Value.NrMat = 1;
					nSBMD.modelSet.models[0].materials.dictTexToMatList[num4].Value.Materials = new int[1]
					{
				i
					};
					nSBMD.modelSet.models[0].materials.dictPlttToMatList[num4].Value.NrMat = 1;
					nSBMD.modelSet.models[0].materials.dictPlttToMatList[num4].Value.Materials = new int[1]
					{
				i
					};
					num4++;
				}
				nSBMD.modelSet.models[0].materials.dict.Add(i.ToString() + "_m", new NSBMD.ModelSet.Model.MaterialSet.MaterialSetData());
				nSBMD.modelSet.models[0].materials.materials[i] = new NSBMD.ModelSet.Model.MaterialSet.Material();
				nSBMD.modelSet.models[0].materials.materials[i].diffAmb = (uint)(((Graphic.encodeColor(Color.Black.ToArgb()) & 0x7FFF) << 16) | 0x8000 | (Graphic.encodeColor((materialByName.DiffuseMap != null) ? Color.FromArgb(200, 200, 200).ToArgb() : materialByName.DiffuseColor.ToArgb()) & 0x7FFF));
				nSBMD.modelSet.models[0].materials.materials[i].specEmi = (uint)(((Graphic.encodeColor(Color.Black.ToArgb()) & 0x7FFF) << 16) | (Graphic.encodeColor(Color.Black.ToArgb()) & 0x7FFF));
				uint num5 = (uint)(materialByName.Alpha * 31f);
				nSBMD.modelSet.models[0].materials.materials[i].polyAttr = 0u;
				nSBMD.modelSet.models[0].materials.materials[i].polyAttr |= 192u;
				nSBMD.modelSet.models[0].materials.materials[i].polyAttr |= num5 << 16;
				nSBMD.modelSet.models[0].materials.materials[i].polyAttrMask = 1059059967u;
				nSBMD.modelSet.models[0].materials.materials[i].texImageParam = 196608u;
				nSBMD.modelSet.models[0].materials.materials[i].texImageParamMask = uint.MaxValue;
				nSBMD.modelSet.models[0].materials.materials[i].texPlttBase = 0;
				nSBMD.modelSet.models[0].materials.materials[i].flag = (NSBMD.ModelSet.Model.MaterialSet.Material.NNS_G3D_MATFLAG.NNS_G3D_MATFLAG_TEXMTX_SCALEONE | NSBMD.ModelSet.Model.MaterialSet.Material.NNS_G3D_MATFLAG.NNS_G3D_MATFLAG_TEXMTX_ROTZERO | NSBMD.ModelSet.Model.MaterialSet.Material.NNS_G3D_MATFLAG.NNS_G3D_MATFLAG_TEXMTX_TRANSZERO | NSBMD.ModelSet.Model.MaterialSet.Material.NNS_G3D_MATFLAG.NNS_G3D_MATFLAG_DIFFUSE | NSBMD.ModelSet.Model.MaterialSet.Material.NNS_G3D_MATFLAG.NNS_G3D_MATFLAG_AMBIENT | NSBMD.ModelSet.Model.MaterialSet.Material.NNS_G3D_MATFLAG.NNS_G3D_MATFLAG_VTXCOLOR | NSBMD.ModelSet.Model.MaterialSet.Material.NNS_G3D_MATFLAG.NNS_G3D_MATFLAG_SPECULAR | NSBMD.ModelSet.Model.MaterialSet.Material.NNS_G3D_MATFLAG.NNS_G3D_MATFLAG_EMISSION | NSBMD.ModelSet.Model.MaterialSet.Material.NNS_G3D_MATFLAG.NNS_G3D_MATFLAG_SHININESS);
				if (materialByName.DiffuseMap != null)
				{
					nSBMD.modelSet.models[0].materials.materials[i].origWidth = (ushort)materialByName.DiffuseMap.Width;
					nSBMD.modelSet.models[0].materials.materials[i].origHeight = (ushort)materialByName.DiffuseMap.Height;
				}
				nSBMD.modelSet.models[0].materials.materials[i].magW = 1f;
				nSBMD.modelSet.models[0].materials.materials[i].magH = 1f;
				i++;
			}
			nSBMD.modelSet.models[0].shapes = new NSBMD.ModelSet.Model.ShapeSet();
			nSBMD.modelSet.models[0].shapes.dict = new Dictionary<NSBMD.ModelSet.Model.ShapeSet.ShapeSetData>();
			nSBMD.modelSet.models[0].shapes.shape = new NSBMD.ModelSet.Model.ShapeSet.Shape[list.Count];
			i = 0;
			foreach (string item2 in list)
			{
				int num6 = 1;
				int num7 = 1;
				MLT.Material materialByName = mLT.GetMaterialByName(item2);
				List<Color> list2 = null;
				if (materialByName.DiffuseMap != null)
				{
					num6 = materialByName.DiffuseMap.Width;
					num7 = -materialByName.DiffuseMap.Height;
				}
				else
				{
					list2 = new List<Color>();
				}
				nSBMD.modelSet.models[0].shapes.dict.Add(i.ToString() + "_py", new NSBMD.ModelSet.Model.ShapeSet.ShapeSetData());
				List<Vector3> list3 = new List<Vector3>();
				List<Vector2> list4 = new List<Vector2>();
				List<Vector3> list5 = new List<Vector3>();
				foreach (OBJ.Face face2 in oBJ.Faces)
				{
					if (face2.MaterialName == item2)
					{
						list3.AddRange(new Vector3[3]
						{
					oBJ.Vertices[face2.VertexIndieces[0]],
					oBJ.Vertices[face2.VertexIndieces[1]],
					oBJ.Vertices[face2.VertexIndieces[2]]
						});
						Vector2[] array = new Vector2[3]
						{
					oBJ.TexCoords[face2.TexCoordIndieces[0]],
					oBJ.TexCoords[face2.TexCoordIndieces[1]],
					oBJ.TexCoords[face2.TexCoordIndieces[2]]
						};
						array[0].X *= num6;
						array[0].Y *= num7;
						array[1].X *= num6;
						array[1].Y *= num7;
						array[2].X *= num6;
						array[2].Y *= num7;
						list4.AddRange(array);
						if (face2.NormalIndieces.Count != 0)
						{
							list5.AddRange(new Vector3[3]
							{
						oBJ.Normals[face2.NormalIndieces[0]],
						oBJ.Normals[face2.NormalIndieces[1]],
						oBJ.Normals[face2.NormalIndieces[2]]
							});
						}
						list2?.AddRange(new Color[3]
						{
					materialByName.DiffuseColor,
					materialByName.DiffuseColor,
					materialByName.DiffuseColor
						});
					}
				}
				nSBMD.modelSet.models[0].shapes.shape[i] = new NSBMD.ModelSet.Model.ShapeSet.Shape();
				nSBMD.modelSet.models[0].shapes.shape[i].DL = GlNitro.GenerateDl(list3.ToArray(), list4.ToArray(), list5.ToArray(), list2?.ToArray());
				nSBMD.modelSet.models[0].shapes.shape[i].sizeDL = (uint)nSBMD.modelSet.models[0].shapes.shape[i].DL.Length;
				nSBMD.modelSet.models[0].shapes.shape[i].flag = (NSBMD.ModelSet.Model.ShapeSet.Shape.NNS_G3D_SHPFLAG.NNS_G3D_SHPFLAG_USE_NORMAL | NSBMD.ModelSet.Model.ShapeSet.Shape.NNS_G3D_SHPFLAG.NNS_G3D_SHPFLAG_USE_COLOR | NSBMD.ModelSet.Model.ShapeSet.Shape.NNS_G3D_SHPFLAG.NNS_G3D_SHPFLAG_USE_TEXCOORD);
				i++;
			}
			var sBCWriter = new Obj2Nsbmd.SBCWriter();
			sBCWriter.NODEDESC(0, 0, S: false, P: false, 0);
			sBCWriter.NODE(0, V: true);
			sBCWriter.POSSCALE(OPT: true);
			for (i = 0; i < list.Count; i++)
			{
				sBCWriter.MAT((byte)i);
				sBCWriter.SHP((byte)i);
			}
			sBCWriter.POSSCALE(OPT: false);
			sBCWriter.RET();
			sBCWriter.NOP();
			nSBMD.modelSet.models[0].sbc = sBCWriter.GetData();
			NSBTX.TexplttSet texplttSet = new NSBTX.TexplttSet();
			texplttSet.TexInfo = new NSBTX.TexplttSet.texInfo();
			texplttSet.Tex4x4Info = new NSBTX.TexplttSet.tex4x4Info();
			texplttSet.PlttInfo = new NSBTX.TexplttSet.plttInfo();
			texplttSet.dictTex = new Dictionary<NSBTX.TexplttSet.DictTexData>();
			texplttSet.dictPltt = new Dictionary<NSBTX.TexplttSet.DictPlttData>();
			i = 0;
			num4 = 0;
			foreach (string item3 in list)
			{
				MLT.Material materialByName = mLT.GetMaterialByName(item3);
				if (materialByName.DiffuseMap != null)
				{
					BitmapData bitmapData = materialByName.DiffuseMap.LockBits(new Rectangle(0, 0, materialByName.DiffuseMap.Width, materialByName.DiffuseMap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
					List<Color> list6 = new List<Color>();
					bool flag = false;
					for (int j = 0; j < materialByName.DiffuseMap.Width * materialByName.DiffuseMap.Height; j++)
					{
						list6.Add(Color.FromArgb(Marshal.ReadInt32(bitmapData.Scan0, j * 4)));
						if (list6.Last().A != 0 && list6.Last().A != byte.MaxValue && !flag)
						{
							flag = true;
						}
					}
					materialByName.DiffuseMap.UnlockBits(bitmapData);
					list6 = list6.Distinct().ToList();
					texplttSet.dictTex.Add(i.ToString() + "_t", new NSBTX.TexplttSet.DictTexData());
					texplttSet.dictPltt.Add(i.ToString() + "_p", new NSBTX.TexplttSet.DictPlttData());
					texplttSet.dictTex[num4].Value.S = (ushort)materialByName.DiffuseMap.Width;
					texplttSet.dictTex[num4].Value.T = (ushort)materialByName.DiffuseMap.Height;
					if (flag && list6.Count <= 8)
					{
						texplttSet.dictTex[num4].Value.Fmt = Graphic.GXTexFmt.GX_TEXFMT_A5I3;
					}
					else if (flag)
					{
						texplttSet.dictTex[num4].Value.Fmt = Graphic.GXTexFmt.GX_TEXFMT_A3I5;
					}
					else if (list6.Count <= 16)
					{
						texplttSet.dictTex[num4].Value.Fmt = Graphic.GXTexFmt.GX_TEXFMT_PLTT16;
					}
					else
					{
						texplttSet.dictTex[num4].Value.Fmt = Graphic.GXTexFmt.GX_TEXFMT_COMP4x4;
					}
					Graphic.ConvertBitmap(materialByName.DiffuseMap, out texplttSet.dictTex[num4].Value.Data, out texplttSet.dictPltt[num4].Value.Data, out texplttSet.dictTex[num4].Value.Data4x4, texplttSet.dictTex[num4].Value.Fmt, Graphic.NNSG2dCharacterFmt.NNS_G2D_CHARACTER_FMT_BMP, out texplttSet.dictTex[num4].Value.TransparentColor);
					num4++;
				}
				i++;
			}
			nsbtx = null;
			if (!has_texture)
			{
				nSBMD.TexPlttSet = texplttSet;
			}
			else
			{
                var nSBTX = new NSBTX
                {
                    TexPlttSet = texplttSet
                };
                nsbtx = nSBTX.Write();
			}
			nsbmd = nSBMD.Write();
		}

		public static void ScaleOBJ(string in_obj, string out_obj, float scale)
        {
			var obj = new OBJ(in_obj);
			for(int i = 0; i < obj.Vertices.Count; i++)
            {
				obj.Vertices[i] = Vector3.Multiply(obj.Vertices[i], scale);
            }
			obj.Write(out_obj);
        }
    }
}
