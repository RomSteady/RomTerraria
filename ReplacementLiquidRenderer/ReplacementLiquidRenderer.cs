using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Threading;
using Terraria;
using Terraria.Graphics;
using Terraria.Utilities;

// Decompiled using Telerik JustDecompile

namespace Terraria.GameContent.Liquid
{
    public class ReplacementLiquidRenderer
    {
        private const int ANIMATION_FRAME_COUNT = 16;

        private const int CACHE_PADDING = 2;

        private const int CACHE_PADDING_2 = 4;

        public const float MIN_LIQUID_SIZE = 0.25f;

        private const int WAVE_MASK_SIZE = 200;

        private readonly static int[] WATERFALL_LENGTH;

        private readonly static float[] DEFAULT_OPACITY;

        private readonly static byte[] WAVE_MASK_STRENGTH;

        private readonly static byte[] VISCOSITY_MASK;

        public static ReplacementLiquidRenderer Instance;

        private Tile[,] _tiles = Main.tile;

        private Texture2D[] _liquidTextures = new Texture2D[12];

        private ReplacementLiquidRenderer.LiquidCache[] _cache = new ReplacementLiquidRenderer.LiquidCache[41617];

        private ReplacementLiquidRenderer.LiquidDrawCache[] _drawCache = new ReplacementLiquidRenderer.LiquidDrawCache[40001];

        private int _animationFrame;

        private Rectangle _drawArea;

        private UnifiedRandom _random = new UnifiedRandom();

        private Color[] _waveMask = new Color[40000];

        private float _frameState;

        static ReplacementLiquidRenderer()
        {
            ReplacementLiquidRenderer.WATERFALL_LENGTH = new int[] { 10, 3, 2 };
            ReplacementLiquidRenderer.DEFAULT_OPACITY = new float[] { 0.6f, 0.95f, 0.95f };
            byte[] numArray = new byte[] { 0, 0, 0, 255, 0 };
            ReplacementLiquidRenderer.WAVE_MASK_STRENGTH = numArray;
            byte[] numArray1 = new byte[] { 0, 200, 240, 0, 0 };
            ReplacementLiquidRenderer.VISCOSITY_MASK = numArray1;
            ReplacementLiquidRenderer.Instance = new ReplacementLiquidRenderer();
        }

        public ReplacementLiquidRenderer()
        {
            for (int i = 0; i < (int)this._liquidTextures.Length; i++)
            {
                this._liquidTextures[i] = TextureManager.Load(string.Concat("Images/Misc/water_", i));
            }
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 drawOffset, int waterStyle, float alpha, bool isBackgroundDraw)
        {
            this.InternalDraw(spriteBatch, drawOffset, waterStyle, alpha, isBackgroundDraw);
        }

        public Rectangle GetCachedDrawArea()
        {
            return this._drawArea;
        }

        public float GetVisibleLiquid(int x, int y)
        {
            x = x - this._drawArea.X;
            y = y - this._drawArea.Y;
            if (x < 0 || x >= this._drawArea.Width || y < 0 || y >= this._drawArea.Height)
            {
                return 0f;
            }
            int num = (x + 2) * (this._drawArea.Height + 4) + y + 2;
            if (!this._cache[num].HasVisibleLiquid)
            {
                return 0f;
            }
            return this._cache[num].VisibleLiquidLevel;
        }

        public bool HasFullWater(int x, int y)
        {
            x = x - this._drawArea.X;
            y = y - this._drawArea.Y;
            int num = x * this._drawArea.Height + y;
            if (num < 0 || num >= (int)this._drawCache.Length)
            {
                return true;
            }
            if (!this._drawCache[num].IsVisible)
            {
                return false;
            }
            return !this._drawCache[num].IsSurfaceLiquid;
        }

        private unsafe void InternalDraw(SpriteBatch spriteBatch, Vector2 drawOffset, int waterStyle, float globalAlpha, bool isBackgroundDraw)
        {
            VertexColors bottomLeftColor;
            Rectangle rectangle = this._drawArea;
            Main.tileBatch.Begin();
            fixed (ReplacementLiquidRenderer.LiquidDrawCache* liquidDrawCachePointer = &this._drawCache[0])
            {
                ReplacementLiquidRenderer.LiquidDrawCache* liquidDrawCachePointer1 = liquidDrawCachePointer;
                for (int i = rectangle.X; i < rectangle.X + rectangle.Width; i++)
                {
                    for (int j = rectangle.Y; j < rectangle.Y + rectangle.Height; j++)
                    {
                        if ((*liquidDrawCachePointer1).IsVisible)
                        {
                            Rectangle sourceRectangle = (*liquidDrawCachePointer1).SourceRectangle;
                            if (!(*liquidDrawCachePointer1).IsSurfaceLiquid)
                            {
                                sourceRectangle.Y = sourceRectangle.Y + this._animationFrame * 80;
                            }
                            else
                            {
                                sourceRectangle.Y = 1280;
                            }
                            Vector2 liquidOffset = (*liquidDrawCachePointer1).LiquidOffset;
                            float opacity = (*liquidDrawCachePointer1).Opacity * (isBackgroundDraw ? 1f : ReplacementLiquidRenderer.DEFAULT_OPACITY[(*liquidDrawCachePointer1).Type]);
                            int type = (*liquidDrawCachePointer1).Type;
                            if (type == 0)
                            {
                                type = waterStyle;
                                opacity = opacity * (isBackgroundDraw ? 1f : globalAlpha);
                            }
                            else if (type == 2)
                            {
                                type = 11;
                            }
                            opacity = Math.Min(1f, opacity);
                            Lighting.GetColor4Slice_New(i, j, out bottomLeftColor, 1f);
                            bottomLeftColor.BottomLeftColor = bottomLeftColor.BottomLeftColor * opacity;
                            bottomLeftColor.BottomRightColor = bottomLeftColor.BottomRightColor * opacity;
                            bottomLeftColor.TopLeftColor = bottomLeftColor.TopLeftColor * opacity;
                            bottomLeftColor.TopRightColor = bottomLeftColor.TopRightColor * opacity;
                            Main.tileBatch.Draw(this._liquidTextures[type], (new Vector2((float)(i << 4), (float)(j << 4)) + drawOffset) + liquidOffset, new Rectangle?(sourceRectangle), bottomLeftColor, Vector2.Zero, 1f, SpriteEffects.None);
                        }
                        liquidDrawCachePointer1 = liquidDrawCachePointer1 + sizeof(ReplacementLiquidRenderer.LiquidDrawCache);
                    }
                }
            }
            Main.tileBatch.End();
        }

        private unsafe void InternalPrepareDraw(Rectangle drawArea)
        {
            ReplacementLiquidRenderer.LiquidCache height;
            ReplacementLiquidRenderer.LiquidCache liquidCache;
            ReplacementLiquidRenderer.LiquidCache height1;
            ReplacementLiquidRenderer.LiquidCache liquidCache1;
            bool flag;
            Rectangle rectangle = new Rectangle(drawArea.X - 2, drawArea.Y - 2, drawArea.Width + 4, drawArea.Height + 4);
            this._drawArea = drawArea;
            Tile tile = null;
            fixed (ReplacementLiquidRenderer.LiquidCache* liquidCachePointer = &this._cache[1])
            {
                ReplacementLiquidRenderer.LiquidCache* type = liquidCachePointer;
                int num = rectangle.Height * 2 + 2;
                type = liquidCachePointer;
                for (int i = rectangle.X; i < rectangle.X + rectangle.Width; i++)
                {
                    for (int j = rectangle.Y; j < rectangle.Y + rectangle.Height; j++)
                    {
                        tile = this._tiles[i, j] ?? new Tile();
                        (*type).LiquidLevel = (float)tile.liquid / 255f;
                        (*type).IsHalfBrick = (!tile.halfBrick() ? false : (*(type + -1 * sizeof(ReplacementLiquidRenderer.LiquidCache))).HasLiquid);
                        (*type).IsSolid = (!WorldGen.SolidOrSlopedTile(tile) ? false : !(*type).IsHalfBrick);
                        (*type).HasLiquid = tile.liquid != 0;
                        (*type).VisibleLiquidLevel = 0f;
                        (*type).HasWall = tile.wall != 0;
                        (*type).Type = tile.liquidType();
                        if ((*type).IsHalfBrick && !(*type).HasLiquid)
                        {
                            (*type).Type = (*(type + -1 * sizeof(ReplacementLiquidRenderer.LiquidCache))).Type;
                        }
                        type = type + sizeof(ReplacementLiquidRenderer.LiquidCache);
                    }
                }
                type = liquidCachePointer;
                float liquidLevel = 0f;
                type = type + num * sizeof(ReplacementLiquidRenderer.LiquidCache);
                for (int k = 2; k < rectangle.Width - 2; k++)
                {
                    for (int l = 2; l < rectangle.Height - 2; l++)
                    {
                        liquidLevel = 0f;
                        if ((*type).IsHalfBrick && (*(type + -1 * sizeof(ReplacementLiquidRenderer.LiquidCache))).HasLiquid)
                        {
                            liquidLevel = 1f;
                        }
                        else if ((*type).HasLiquid)
                        {
                            liquidLevel = (*type).LiquidLevel;
                        }
                        else
                        {
                            height = *(type + -rectangle.Height * sizeof(ReplacementLiquidRenderer.LiquidCache));
                            liquidCache = *(type + rectangle.Height * sizeof(ReplacementLiquidRenderer.LiquidCache));
                            height1 = *(type + -1 * sizeof(ReplacementLiquidRenderer.LiquidCache));
                            liquidCache1 = *(type + sizeof(ReplacementLiquidRenderer.LiquidCache));
                            if (height.HasLiquid && liquidCache.HasLiquid && height.Type == liquidCache.Type)
                            {
                                liquidLevel = height.LiquidLevel + liquidCache.LiquidLevel;
                                (*type).Type = height.Type;
                            }
                            if (height1.HasLiquid && liquidCache1.HasLiquid && height1.Type == liquidCache1.Type)
                            {
                                liquidLevel = Math.Max(liquidLevel, height1.LiquidLevel + liquidCache1.LiquidLevel);
                                (*type).Type = height1.Type;
                            }
                            liquidLevel = liquidLevel * 0.5f;
                        }
                        (*type).VisibleLiquidLevel = liquidLevel;
                        (*type).HasVisibleLiquid = liquidLevel != 0f;
                        type = type + sizeof(ReplacementLiquidRenderer.LiquidCache);
                    }
                    type = type + 4 * sizeof(ReplacementLiquidRenderer.LiquidCache);
                }
                type = liquidCachePointer;
                for (int m = 0; m < rectangle.Width; m++)
                {
                    for (int n = 0; n < rectangle.Height - 10; n++)
                    {
                        if ((*type).HasVisibleLiquid && !(*type).IsSolid)
                        {
                            (*type).Opacity = 1f;
                            (*type).VisibleType = (*type).Type;
                            float wATERFALLLENGTH = 1f / (float)(ReplacementLiquidRenderer.WATERFALL_LENGTH[(*type).Type] + 1);
                            float single = 1f;
                            for (int o = 1; o <= ReplacementLiquidRenderer.WATERFALL_LENGTH[(*type).Type]; o++)
                            {
                                single = single - wATERFALLLENGTH;
                                if ((*(type + o * sizeof(ReplacementLiquidRenderer.LiquidCache))).IsSolid)
                                {
                                    break;
                                }
                                (*(type + o * sizeof(ReplacementLiquidRenderer.LiquidCache))).VisibleLiquidLevel = Math.Max((*(type + o * sizeof(ReplacementLiquidRenderer.LiquidCache))).VisibleLiquidLevel, (*type).VisibleLiquidLevel * single);
                                (*(type + o * sizeof(ReplacementLiquidRenderer.LiquidCache))).Opacity = single;
                                (*(type + o * sizeof(ReplacementLiquidRenderer.LiquidCache))).VisibleType = (*type).Type;
                            }
                        }
                        if (!(*type).IsSolid)
                        {
                            (*type).HasVisibleLiquid = (*type).VisibleLiquidLevel != 0f;
                        }
                        else
                        {
                            (*type).VisibleLiquidLevel = 1f;
                            (*type).HasVisibleLiquid = false;
                        }
                        type = type + sizeof(ReplacementLiquidRenderer.LiquidCache);
                    }
                    type = type + 10 * sizeof(ReplacementLiquidRenderer.LiquidCache);
                }
                type = liquidCachePointer;
                type = type + num * sizeof(ReplacementLiquidRenderer.LiquidCache);
                for (int p = 2; p < rectangle.Width - 2; p++)
                {
                    for (int q = 2; q < rectangle.Height - 2; q++)
                    {
                        if (!(*type).HasVisibleLiquid || (*type).IsSolid)
                        {
                            (*type).HasLeftEdge = false;
                            (*type).HasTopEdge = false;
                            (*type).HasRightEdge = false;
                            (*type).HasBottomEdge = false;
                        }
                        else
                        {
                            height = *(type + -1 * sizeof(ReplacementLiquidRenderer.LiquidCache));
                            liquidCache = *(type + sizeof(ReplacementLiquidRenderer.LiquidCache));
                            height1 = *(type + -rectangle.Height * sizeof(ReplacementLiquidRenderer.LiquidCache));
                            liquidCache1 = *(type + rectangle.Height * sizeof(ReplacementLiquidRenderer.LiquidCache));
                            float visibleLiquidLevel = 0f;
                            float visibleLiquidLevel1 = 1f;
                            float single1 = 0f;
                            float visibleLiquidLevel2 = 1f;
                            float single2 = (*type).VisibleLiquidLevel;
                            if (!height.HasVisibleLiquid)
                            {
                                single1 = single1 + liquidCache.VisibleLiquidLevel * (1f - single2);
                            }
                            if (!liquidCache.HasVisibleLiquid && !liquidCache.IsSolid && !liquidCache.IsHalfBrick)
                            {
                                visibleLiquidLevel2 = visibleLiquidLevel2 - height.VisibleLiquidLevel * (1f - single2);
                            }
                            if (!height1.HasVisibleLiquid && !height1.IsSolid && !height1.IsHalfBrick)
                            {
                                visibleLiquidLevel = visibleLiquidLevel + liquidCache1.VisibleLiquidLevel * (1f - single2);
                            }
                            if (!liquidCache1.HasVisibleLiquid && !liquidCache1.IsSolid && !liquidCache1.IsHalfBrick)
                            {
                                visibleLiquidLevel1 = visibleLiquidLevel1 - height1.VisibleLiquidLevel * (1f - single2);
                            }
                            (*type).LeftWall = visibleLiquidLevel;
                            (*type).RightWall = visibleLiquidLevel1;
                            (*type).BottomWall = visibleLiquidLevel2;
                            (*type).TopWall = single1;
                            Point zero = Point.Zero;
                            (*type).HasTopEdge = (height.HasVisibleLiquid || height.IsSolid ? single1 != 0f : true);
                            (*type).HasBottomEdge = (liquidCache.HasVisibleLiquid || liquidCache.IsSolid ? visibleLiquidLevel2 != 1f : true);
                            (*type).HasLeftEdge = (height1.HasVisibleLiquid || height1.IsSolid ? visibleLiquidLevel != 0f : true);
                            (*type).HasRightEdge = (liquidCache1.HasVisibleLiquid || liquidCache1.IsSolid ? visibleLiquidLevel1 != 1f : true);
                            if (!(*type).HasLeftEdge)
                            {
                                if (!(*type).HasRightEdge)
                                {
                                    zero.X = zero.X + 16;
                                }
                                else
                                {
                                    zero.X = zero.X + 32;
                                }
                            }
                            if ((*type).HasLeftEdge && (*type).HasRightEdge)
                            {
                                zero.X = 16;
                                zero.Y = zero.Y + 32;
                                if ((*type).HasTopEdge)
                                {
                                    zero.Y = 16;
                                }
                            }
                            else if (!(*type).HasTopEdge)
                            {
                                if ((*type).HasLeftEdge || (*type).HasRightEdge)
                                {
                                    zero.Y = zero.Y + 16;
                                }
                                else
                                {
                                    zero.Y = zero.Y + 48;
                                }
                            }
                            if (zero.Y == 16 && (*type).HasLeftEdge ^ (*type).HasRightEdge && q + rectangle.Y % 2 == 0)
                            {
                                zero.Y = zero.Y + 16;
                            }
                            (*type).FrameOffset = zero;
                        }
                        type = type + sizeof(ReplacementLiquidRenderer.LiquidCache);
                    }
                    type = type + 4 * sizeof(ReplacementLiquidRenderer.LiquidCache);
                }
                type = liquidCachePointer;
                type = type + num * sizeof(ReplacementLiquidRenderer.LiquidCache);
                for (int r = 2; r < rectangle.Width - 2; r++)
                {
                    for (int s = 2; s < rectangle.Height - 2; s++)
                    {
                        if ((*type).HasVisibleLiquid)
                        {
                            height = *(type + -1 * sizeof(ReplacementLiquidRenderer.LiquidCache));
                            liquidCache = *(type + sizeof(ReplacementLiquidRenderer.LiquidCache));
                            height1 = *(type + -rectangle.Height * sizeof(ReplacementLiquidRenderer.LiquidCache));
                            liquidCache1 = *(type + rectangle.Height * sizeof(ReplacementLiquidRenderer.LiquidCache));
                            (*type).VisibleLeftWall = (*type).LeftWall;
                            (*type).VisibleRightWall = (*type).RightWall;
                            (*type).VisibleTopWall = (*type).TopWall;
                            (*type).VisibleBottomWall = (*type).BottomWall;
                            if (height.HasVisibleLiquid && liquidCache.HasVisibleLiquid)
                            {
                                if ((*type).HasLeftEdge)
                                {
                                    (*type).VisibleLeftWall = ((*type).LeftWall * 2f + height.LeftWall + liquidCache.LeftWall) * 0.25f;
                                }
                                if ((*type).HasRightEdge)
                                {
                                    (*type).VisibleRightWall = ((*type).RightWall * 2f + height.RightWall + liquidCache.RightWall) * 0.25f;
                                }
                            }
                            if (height1.HasVisibleLiquid && liquidCache1.HasVisibleLiquid)
                            {
                                if ((*type).HasTopEdge)
                                {
                                    (*type).VisibleTopWall = ((*type).TopWall * 2f + height1.TopWall + liquidCache1.TopWall) * 0.25f;
                                }
                                if ((*type).HasBottomEdge)
                                {
                                    (*type).VisibleBottomWall = ((*type).BottomWall * 2f + height1.BottomWall + liquidCache1.BottomWall) * 0.25f;
                                }
                            }
                        }
                        type = type + sizeof(ReplacementLiquidRenderer.LiquidCache);
                    }
                    type = type + 4 * sizeof(ReplacementLiquidRenderer.LiquidCache);
                }
                type = liquidCachePointer;
                type = type + num * sizeof(ReplacementLiquidRenderer.LiquidCache);
                for (int t = 2; t < rectangle.Width - 2; t++)
                {
                    for (int u = 2; u < rectangle.Height - 2; u++)
                    {
                        if ((*type).HasLiquid)
                        {
                            height = *(type + -1 * sizeof(ReplacementLiquidRenderer.LiquidCache));
                            liquidCache = *(type + sizeof(ReplacementLiquidRenderer.LiquidCache));
                            height1 = *(type + -rectangle.Height * sizeof(ReplacementLiquidRenderer.LiquidCache));
                            liquidCache1 = *(type + rectangle.Height * sizeof(ReplacementLiquidRenderer.LiquidCache));
                            if ((*type).HasTopEdge && !(*type).HasBottomEdge && (*type).HasLeftEdge ^ (*type).HasRightEdge)
                            {
                                if (!(*type).HasRightEdge)
                                {
                                    (*type).VisibleLeftWall = liquidCache.VisibleLeftWall;
                                    (*type).VisibleTopWall = liquidCache1.VisibleTopWall;
                                }
                                else
                                {
                                    (*type).VisibleRightWall = liquidCache.VisibleRightWall;
                                    (*type).VisibleTopWall = height1.VisibleTopWall;
                                }
                            }
                            else if (liquidCache.FrameOffset.X == 16 && liquidCache.FrameOffset.Y == 32)
                            {
                                if ((*type).VisibleLeftWall > 0.5f)
                                {
                                    (*type).VisibleLeftWall = 0f;
                                    (*type).FrameOffset = new Point(0, 0);
                                }
                                else if ((*type).VisibleRightWall < 0.5f)
                                {
                                    (*type).VisibleRightWall = 1f;
                                    (*type).FrameOffset = new Point(32, 0);
                                }
                            }
                        }
                        type = type + sizeof(ReplacementLiquidRenderer.LiquidCache);
                    }
                    type = type + 4 * sizeof(ReplacementLiquidRenderer.LiquidCache);
                }
                type = liquidCachePointer;
                type = type + num * sizeof(ReplacementLiquidRenderer.LiquidCache);
                for (int v = 2; v < rectangle.Width - 2; v++)
                {
                    for (int w = 2; w < rectangle.Height - 2; w++)
                    {
                        if ((*type).HasLiquid)
                        {
                            height = *(type + -1 * sizeof(ReplacementLiquidRenderer.LiquidCache));
                            liquidCache = *(type + sizeof(ReplacementLiquidRenderer.LiquidCache));
                            height1 = *(type + -rectangle.Height * sizeof(ReplacementLiquidRenderer.LiquidCache));
                            liquidCache1 = *(type + rectangle.Height * sizeof(ReplacementLiquidRenderer.LiquidCache));
                            if (!(*type).HasBottomEdge && !(*type).HasLeftEdge && !(*type).HasTopEdge && !(*type).HasRightEdge)
                            {
                                if (height1.HasTopEdge && height.HasLeftEdge)
                                {
                                    (*type).FrameOffset.X = Math.Max(4, (int)(16f - height.VisibleLeftWall * 16f)) - 4;
                                    (*type).FrameOffset.Y = 48 + Math.Max(4, (int)(16f - height1.VisibleTopWall * 16f)) - 4;
                                    (*type).VisibleLeftWall = 0f;
                                    (*type).VisibleTopWall = 0f;
                                    (*type).VisibleRightWall = 1f;
                                    (*type).VisibleBottomWall = 1f;
                                }
                                else if (liquidCache1.HasTopEdge && height.HasRightEdge)
                                {
                                    (*type).FrameOffset.X = 32 - Math.Min(16, (int)(height.VisibleRightWall * 16f) - 4);
                                    (*type).FrameOffset.Y = 48 + Math.Max(4, (int)(16f - liquidCache1.VisibleTopWall * 16f)) - 4;
                                    (*type).VisibleLeftWall = 0f;
                                    (*type).VisibleTopWall = 0f;
                                    (*type).VisibleRightWall = 1f;
                                    (*type).VisibleBottomWall = 1f;
                                }
                            }
                        }
                        type = type + sizeof(ReplacementLiquidRenderer.LiquidCache);
                    }
                    type = type + 4 * sizeof(ReplacementLiquidRenderer.LiquidCache);
                }
                type = liquidCachePointer;
                type = type + num * sizeof(ReplacementLiquidRenderer.LiquidCache);
                fixed (ReplacementLiquidRenderer.LiquidDrawCache* liquidDrawCachePointer = &this._drawCache[0])
                {
                    fixed (Color* colorPointer = &this._waveMask[0])
                    {
                        ReplacementLiquidRenderer.LiquidDrawCache* opacity = liquidDrawCachePointer;
                        Color* vISCOSITYMASK = colorPointer;
                        for (int x = 2; x < rectangle.Width - 2; x++)
                        {
                            Color* colorPointer1 = vISCOSITYMASK;
                            for (int y = 2; y < rectangle.Height - 2; y++)
                            {
                                if (!(*type).HasVisibleLiquid)
                                {
                                    (*opacity).IsVisible = false;
                                    int num1 = ((*type).IsSolid || (*type).IsHalfBrick ? 3 : 4);
                                    byte wAVEMASKSTRENGTH = ReplacementLiquidRenderer.WAVE_MASK_STRENGTH[num1];
                                    byte num2 = (byte)(wAVEMASKSTRENGTH >> 1);
                                    (*vISCOSITYMASK).R = num2;
                                    (*vISCOSITYMASK).G = num2;
                                    (*vISCOSITYMASK).B = ReplacementLiquidRenderer.VISCOSITY_MASK[num1];
                                    (*vISCOSITYMASK).A = wAVEMASKSTRENGTH;
                                }
                                else
                                {
                                    float single3 = Math.Min(0.75f, (*type).VisibleLeftWall);
                                    float single4 = Math.Max(0.25f, (*type).VisibleRightWall);
                                    float single5 = Math.Min(0.75f, (*type).VisibleTopWall);
                                    float single6 = Math.Max(0.25f, (*type).VisibleBottomWall);
                                    if ((*type).IsHalfBrick && single6 > 0.5f)
                                    {
                                        single6 = 0.5f;
                                    }
                                    ReplacementLiquidRenderer.LiquidDrawCache* liquidDrawCachePointer1 = opacity;
                                    if ((*type).HasWall)
                                    {
                                        flag = true;
                                    }
                                    else
                                    {
                                        flag = (!(*type).IsHalfBrick ? true : !(*type).HasLiquid);
                                    }
                                    (*liquidDrawCachePointer1).IsVisible = flag;
                                    (*opacity).SourceRectangle = new Rectangle((int)(16f - single4 * 16f) + (*type).FrameOffset.X, (int)(16f - single6 * 16f) + (*type).FrameOffset.Y, (int)Math.Ceiling((double)((single4 - single3) * 16f)), (int)Math.Ceiling((double)((single6 - single5) * 16f)));
                                    (*opacity).IsSurfaceLiquid = ((*type).FrameOffset.X != 16 || (*type).FrameOffset.Y != 0 ? false : (double)(y + rectangle.Y) > Main.worldSurface - 40);
                                    (*opacity).Opacity = (*type).Opacity;
                                    (*opacity).LiquidOffset = new Vector2((float)Math.Floor((double)(single3 * 16f)), (float)Math.Floor((double)(single5 * 16f)));
                                    (*opacity).Type = (*type).VisibleType;
                                    (*opacity).HasWall = (*type).HasWall;
                                    byte wAVEMASKSTRENGTH1 = ReplacementLiquidRenderer.WAVE_MASK_STRENGTH[(*type).VisibleType];
                                    byte num3 = (byte)(wAVEMASKSTRENGTH1 >> 1);
                                    (*vISCOSITYMASK).R = num3;
                                    (*vISCOSITYMASK).G = num3;
                                    (*vISCOSITYMASK).B = ReplacementLiquidRenderer.VISCOSITY_MASK[(*type).VisibleType];
                                    (*vISCOSITYMASK).A = wAVEMASKSTRENGTH1;
                                    ReplacementLiquidRenderer.LiquidCache* liquidCachePointer1 = type - sizeof(ReplacementLiquidRenderer.LiquidCache);
                                    if (y != 2 && !(*liquidCachePointer1).HasVisibleLiquid && !(*liquidCachePointer1).IsSolid && !(*liquidCachePointer1).IsHalfBrick)
                                    {
                                        *(vISCOSITYMASK - 200 * sizeof(Color)) = *vISCOSITYMASK;
                                    }
                                }
                                type = type + sizeof(ReplacementLiquidRenderer.LiquidCache);
                                opacity = opacity + sizeof(ReplacementLiquidRenderer.LiquidDrawCache);
                                vISCOSITYMASK = vISCOSITYMASK + 200 * sizeof(Color);
                            }
                            type = type + 4 * sizeof(ReplacementLiquidRenderer.LiquidCache);
                            vISCOSITYMASK = colorPointer1 + sizeof(Color);
                        }
                    }
                }
                type = liquidCachePointer;
                for (int a = rectangle.X; a < rectangle.X + rectangle.Width; a++)
                {
                    for (int b = rectangle.Y; b < rectangle.Y + rectangle.Height; b++)
                    {
                        if ((*type).VisibleType == 1 && (*type).HasVisibleLiquid && Dust.lavaBubbles < 200)
                        {
                            if (this._random.Next(700) == 0)
                            {
                                Dust.NewDust(new Vector2((float)(a * 16), (float)(b * 16)), 16, 16, 35, 0f, 0f, 0, Color.White, 1f);
                            }
                            if (this._random.Next(350) == 0)
                            {
                                int num4 = Dust.NewDust(new Vector2((float)(a * 16), (float)(b * 16)), 16, 8, 35, 0f, 0f, 50, Color.White, 1.5f);
                                Dust dust = Main.dust[num4];
                                dust.velocity = dust.velocity * 0.8f;
                                Main.dust[num4].velocity.X = Main.dust[num4].velocity.X * 2f;
                                Main.dust[num4].velocity.Y = Main.dust[num4].velocity.Y - (float)this._random.Next(1, 7) * 0.1f;
                                if (this._random.Next(10) == 0)
                                {
                                    Main.dust[num4].velocity.Y = Main.dust[num4].velocity.Y * (float)this._random.Next(2, 5);
                                }
                                Main.dust[num4].noGravity = true;
                            }
                        }
                        type = type + sizeof(ReplacementLiquidRenderer.LiquidCache);
                    }
                }
            }
            if (this.ViscosityFilters != null)
            {
                this.ViscosityFilters(this._waveMask, this.GetCachedDrawArea());
            }
        }

        public void PrepareDraw(Rectangle drawArea)
        {
            this.InternalPrepareDraw(drawArea);
        }

        public void SetWaveMaskData(Texture2D texture)
        {
            if (this._waveMask.Length != 40000)
            {
                System.Diagnostics.Debugger.Launch();
            }
            texture.SetData<Color>(this._waveMask);
        }

        public void Update(GameTime gameTime)
        {
            if (Main.gamePaused || !Main.hasFocus)
            {
                return;
            }
            float single = Main.windSpeed * 80f;
            single = MathHelper.Clamp(single, -20f, 20f);
            single = (single >= 0f ? Math.Max(10f, single) : Math.Min(-10f, single));
            float single1 = this._frameState;
            TimeSpan elapsedGameTime = gameTime.ElapsedGameTime;
            this._frameState = single1 + single * (float)elapsedGameTime.TotalSeconds;
            while (this._frameState < 0f)
            {
                this._frameState = this._frameState + 16f;
            }
            this._frameState = this._frameState % 16f;
            this._animationFrame = (int)this._frameState;
        }

        public event Action<Color[], Rectangle> ViscosityFilters;

        private struct LiquidCache
        {
            public float LiquidLevel;

            public float VisibleLiquidLevel;

            public float Opacity;

            public bool IsSolid;

            public bool IsHalfBrick;

            public bool HasLiquid;

            public bool HasVisibleLiquid;

            public bool HasWall;

            public Point FrameOffset;

            public bool HasLeftEdge;

            public bool HasRightEdge;

            public bool HasTopEdge;

            public bool HasBottomEdge;

            public float LeftWall;

            public float RightWall;

            public float BottomWall;

            public float TopWall;

            public float VisibleLeftWall;

            public float VisibleRightWall;

            public float VisibleBottomWall;

            public float VisibleTopWall;

            public byte Type;

            public byte VisibleType;
        }

        private struct LiquidDrawCache
        {
            public Rectangle SourceRectangle;

            public Vector2 LiquidOffset;

            public bool IsVisible;

            public float Opacity;

            public byte Type;

            public bool IsSurfaceLiquid;

            public bool HasWall;
        }
    }
}