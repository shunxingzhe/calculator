using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SYD_COPY_FILE
{
    class SpliteMoveIndex
    {
        public enum EN_DIR
        {
            NON,
            HORIZONTAL,
            VERTICAL
        };
        public EN_DIR direct = EN_DIR.NON;//0 无；1 水平；2垂直
        public int rectIndex; //第几个rect
        public int lineIndex; //第几个线 1:上或左；2 下或右
        public int mouseX;
        public int mouseY;


        public static SpliteMoveIndex CreateNon(int x, int y)
        {
            SpliteMoveIndex _nonIndex = new SpliteMoveIndex();
            _nonIndex.direct = EN_DIR.NON;
            _nonIndex.SetMouse(x, y);
            return _nonIndex;
        }

        public SpliteMoveIndex()
        {

        }
        public SpliteMoveIndex(int x, int y)
        {
            SetMouse(x, y);
        }

        public bool IsSameLine(SpliteMoveIndex another)
        {
            return this.direct == another.direct
                && this.rectIndex == another.rectIndex
                && this.lineIndex == another.lineIndex;
        }

        public bool IsIn()
        {
            return direct != EN_DIR.NON;
        }
        public bool IsHorIn()
        {
            return direct == EN_DIR.HORIZONTAL;
        }
        public bool IsVertIn()
        {
            return direct == EN_DIR.VERTICAL;
        }

        public void SetMouse(int x, int y)
        {
            mouseX = x;
            mouseY = y;
        }
    }

    class SpliteRectGroup
    {
        List<Rectangle> _listSplitRect = new List<Rectangle>();
        int _widthSrc;
        int _heightSrc;

        SpliteMoveIndex _lastMoveIndex = new SpliteMoveIndex();

        public int _defaultHitSpace = 5;

        int _moveAllFlagR = 10;
        bool _isMoveAll = false;

        //不参加切割的区域
        List<int> _listSplitRectNotUsed = new List<int>();

        public void SetRect(int widthSrc, int heightSrc, int startX, int startY,
            int widthDest, int heightDest)
        {
            _widthSrc = widthSrc;
            _heightSrc = heightSrc;
            _listSplitRect.Clear();

            GetSplitSize(_widthSrc, _heightSrc, startX, startY,
           widthDest, heightDest, ref _listSplitRect);
        }

        public List<Rectangle> GetRects()
        {
            return _listSplitRect;
        }

        public List<Rectangle> GetRectsSplit()
        {
            List<Rectangle> listShow = new List<Rectangle>();

            int i = 0;
            foreach(Rectangle rect in _listSplitRect)
            {
                if(IsRectUsed(i))
                {
                    listShow.Add(rect);
                }
                i++;
            }
            return listShow;
        }


        public int GetStartX()
        {
            if (_listSplitRect.Count == 0)
                return 0;
            Rectangle first = _listSplitRect.First();
            return first.X;
        }

        public int GetSpliteWidth()
        {
            if (_listSplitRect.Count == 0)
                return 0;
            Rectangle first = _listSplitRect.First();
            return first.Width;
        }

        public int GetSpliteTotalHeight()
        {
            if (_listSplitRect.Count == 0)
                return 0;
            int i = 0;
            foreach (Rectangle r in _listSplitRect)
            {
                i += r.Height;
            }
            return i;
        }

        public void SetMoveAllFlag(bool flag)
        {
            _isMoveAll = flag;
        }

        public bool GetMoveAllFlag()
        {
            return _isMoveAll;
        }

        public int GetStartY()
        {
            if (_listSplitRect.Count == 0)
                return 0;
            Rectangle first = _listSplitRect.First();
            return first.Y;
        }
        public void Draw(Graphics g, bool showinfo)
        {
            SolidBrush brushRect = new SolidBrush(Color.FromArgb(200, 255, 0, 0));
            Font strfont = new Font("Verdana", 12);  //原来是20 设置切割大小以及边框的字体大小和字体为12
            Brush strBrush = Brushes.Blue;
            Brush strBrushBack = Brushes.Transparent;  //原来是White 设置设置切割大小以及边框的字体背景颜色为透明

            //起点圆
            int x = GetStartX();
            int y = GetStartY();
            g.FillEllipse(brushRect, x - _moveAllFlagR, y - _moveAllFlagR, 2 * _moveAllFlagR, 2 * _moveAllFlagR);
            brushRect.Dispose();
            //起点信息
            string startInfo = string.Format("({0}:{1})",x,y);
            SizeF sizeF = g.MeasureString(startInfo, strfont);
            Point ptStart = new Point((int)(x-sizeF.Width/2), (int)(y -sizeF.Height- _defaultHitSpace*2) );
            g.FillRectangle(strBrushBack, new RectangleF(ptStart, sizeF));
            g.DrawString(startInfo, strfont, strBrush, ptStart);


            //画方框
            Color backColor = Color.FromArgb(0, Color.PowderBlue);
            HatchBrush hat1 = new HatchBrush(HatchStyle.OutlinedDiamond, Color.DarkBlue, backColor);
            HatchBrush hat2 = new HatchBrush(HatchStyle.OutlinedDiamond, Color.Red, backColor);

            //输出提示信息
            Pen rectPen = Pens.Red;         
            int i = 0;
            int showIndex = 0;
            string info;
            foreach (Rectangle rect in _listSplitRect)
            {
                i++;
                bool used = IsRectUsed(rect);
                if (used)
                {
                    showIndex++;
                    info = string.Format("{0}-({1}:{2})", showIndex, rect.Width, rect.Height);
                }
                else
                {
                    info = string.Format("({0}:{1})--不参与切割",  rect.Width, rect.Height);
                }

                g.DrawRectangle(rectPen, rect);
                if (!used)
                {
                    g.FillRectangle(hat1, rect);
                }

                Point strStart = new Point(rect.X + 5, rect.Y + 5);            
                sizeF = g.MeasureString(info, strfont);
                if (showinfo == true)
                {
                    g.FillRectangle(strBrushBack, new RectangleF(strStart, sizeF));
                    g.DrawString(info, strfont, strBrush, strStart);
                }
            }
            strfont.Dispose();
            hat1.Dispose();
            hat2.Dispose();
        }
        public bool StartPointMoveTo(int x, int y)
        {
            if (_listSplitRect.Count == 0)
                return false;

            Rectangle first = _listSplitRect.First();
            int moveX = x - first.X;
            int moveY = y - first.Y;

            List<Rectangle> listSplitRectNew = new List<Rectangle>();
            foreach (Rectangle r in _listSplitRect)
            {
                Rectangle tmp = r;
                tmp.Offset(moveX, moveY);
                listSplitRectNew.Add(tmp);
            }

            _listSplitRect.Clear();
            _listSplitRect = listSplitRectNew;
            return true;
        }

        public bool IsAllMove(int mouseX, int mouseY)
        {
            GraphicsPath myGraphicsPath = new GraphicsPath();
            myGraphicsPath.Reset();
            Region myRegion = new Region();

            int x = GetStartX();
            int y = GetStartY();
            myGraphicsPath.AddEllipse(x - _moveAllFlagR, y - _moveAllFlagR, 2 * _moveAllFlagR, 2 * _moveAllFlagR);//points);
            myRegion.MakeEmpty();
            myRegion.Union(myGraphicsPath);
            //返回判断点是否在多边形里
            bool myPoint = myRegion.IsVisible(mouseX, mouseY);
            return myPoint;
        }

        public void ResetMoveFlag()
        {
            _lastMoveIndex.direct = SpliteMoveIndex.EN_DIR.NON;
        }

        public bool SetMove(SpliteMoveIndex index)
        {
            //移动到区域外
            if (!index.IsIn())
            {
                _lastMoveIndex = index;
                return false;
            }

            //不是同一条线
            if (!_lastMoveIndex.IsSameLine(index))
            {
                _lastMoveIndex = index;
                return false;
            }

            //移动到新的区域
            MoveRect(_lastMoveIndex, index);
            _lastMoveIndex = index;
            return true;
        }

        public bool IsInSplite()
        {
            if (_lastMoveIndex == null)
                return false;
            return _lastMoveIndex.IsIn();
        }

        void MoveRect(SpliteMoveIndex last, SpliteMoveIndex now)
        {
            if (last.IsHorIn())
            {
                MoveRectHor(last, now);
            }
            else if (last.IsVertIn())
            {
                MoveRectVert(last, now);
            }
        }

        void MoveRectHor(SpliteMoveIndex last, SpliteMoveIndex now)
        {
            int moveY = now.mouseY - last.mouseY;
            List<Rectangle> listSplitRectNew = new List<Rectangle>();
            int i = 0;
            int find = 0;
            foreach (Rectangle r in _listSplitRect)
            {
                Rectangle tmp = r;
                i++;
                if (find == 2)
                {
                    listSplitRectNew.Add(tmp);
                    continue;
                }
                if (find == 1)
                {
                    tmp.Y += moveY;
                    tmp.Height -= moveY;
                    find = 2;
                    listSplitRectNew.Add(tmp);
                    continue;
                }

                if (i == last.rectIndex)
                {
                    if (last.lineIndex == 1)
                    {
                        tmp.Y += moveY;
                        tmp.Height -= moveY;
                        find = 2;
                        listSplitRectNew.Add(tmp);
                    }
                    else if (last.lineIndex == 2)
                    {
                        tmp.Height += moveY;
                        find = 1;
                        listSplitRectNew.Add(tmp);
                    }
                }
                else
                {
                    listSplitRectNew.Add(tmp);
                }
            }

            _listSplitRect.Clear();
            _listSplitRect = listSplitRectNew;
        }


        void MoveRectVert(SpliteMoveIndex last, SpliteMoveIndex now)
        {
            int moveX = now.mouseX - last.mouseX;
            List<Rectangle> listSplitRectNew = new List<Rectangle>();
            int i = 0;
            foreach (Rectangle r in _listSplitRect)
            {
                Rectangle tmp = r;
                i++;
                if (last.lineIndex == 1)
                {
                    tmp.X += moveX;
                    tmp.Width -= moveX;
                    listSplitRectNew.Add(tmp);
                }
                else if (last.lineIndex == 2)
                {
                    tmp.Width += moveX;
                    listSplitRectNew.Add(tmp);
                }
            }

            _listSplitRect.Clear();
            _listSplitRect = listSplitRectNew;
        }

        SpliteMoveIndex GetHorizontal(int x, int y, int hitSpace)
        {
            int startX = GetStartX();
            int width = GetSpliteWidth();
            if (x < startX || x > (startX + width))
                return SpliteMoveIndex.CreateNon(x, y);

            int i = 0;
            foreach (Rectangle rect in _listSplitRect)
            {
                i++;
                int y1 = rect.Y;
                //是否落在水平线 一定范围内
                if (y >= y1 - hitSpace && y <= (y1 + hitSpace))
                {
                    SpliteMoveIndex moveIndex = new SpliteMoveIndex(x, y);
                    moveIndex.direct = SpliteMoveIndex.EN_DIR.HORIZONTAL;
                    moveIndex.rectIndex = i;
                    moveIndex.lineIndex = 1;
                    return moveIndex;
                }

                int y2 = rect.Y + rect.Height;
                if (y >= (y2 - hitSpace) && y <= (y2 + hitSpace))
                {
                    SpliteMoveIndex moveIndex = new SpliteMoveIndex(x, y);
                    moveIndex.direct = SpliteMoveIndex.EN_DIR.HORIZONTAL;
                    moveIndex.rectIndex = i;
                    moveIndex.lineIndex = 2;
                    return moveIndex;
                }
            }

            return SpliteMoveIndex.CreateNon(x, y);
        }

        SpliteMoveIndex GetVectical(int x, int y, int hitSpace)
        {
            int startY = GetStartY();
            if (y < startY || y > (startY + _heightSrc))
                return SpliteMoveIndex.CreateNon(x, y);

            int i = 0;
            foreach (Rectangle rect in _listSplitRect)
            {
                i++;
                //是否落在垂直线 一定范围内
                if (y >= rect.Y && y <= (rect.Y + rect.Height))
                {
                    int x1 = rect.X;
                    if (x >= (x1 - hitSpace) && x <= (x1 + hitSpace))
                    {
                        SpliteMoveIndex moveIndex = new SpliteMoveIndex(x, y);
                        moveIndex.direct = SpliteMoveIndex.EN_DIR.VERTICAL;
                        moveIndex.rectIndex = i;
                        moveIndex.lineIndex = 1;
                        return moveIndex;
                    }

                    int x2 = rect.X + rect.Width;
                    if (x >= (x2 - hitSpace) && x <= (x2 + hitSpace))
                    {
                        SpliteMoveIndex moveIndex = new SpliteMoveIndex(x, y);
                        moveIndex.direct = SpliteMoveIndex.EN_DIR.VERTICAL;
                        moveIndex.rectIndex = i;
                        moveIndex.lineIndex = 2;
                        return moveIndex;
                    }
                }
            }

            return SpliteMoveIndex.CreateNon(x, y);
        }

        public SpliteMoveIndex PointHit(int x, int y, int hitSpace)
        {
            //判断是否在水平线
            SpliteMoveIndex hRect = GetHorizontal(x, y, hitSpace);
            if (hRect.IsIn())
                return hRect;

            //判断是否在垂直线
            SpliteMoveIndex vRect = GetVectical(x, y, hitSpace);
            if (vRect.IsIn())
                return vRect;

            return SpliteMoveIndex.CreateNon(x, y);
        }

        public bool PointInRect(int x,int y)
        {
            int startX = GetStartX();
            int width = GetSpliteWidth();
            if (x < startX || x > (startX + width))
                return false;

            int startY = GetStartY();
            int heght = GetSpliteTotalHeight();
            if (y < startY || y > (startY + heght))
                return false;
            return true;
        }

        public void ClearNotUsedRect()
        {
            _listSplitRectNotUsed.Clear();
        }

        public void ClearAllRect()
        {
            _listSplitRect.Clear();
        }

        public bool GetRectIndex(int index,ref Rectangle outRect)
        {
            int i = 0;
            foreach (Rectangle rect in _listSplitRect)
            {
                if (i == index)
                {
                    outRect = rect;
                    return true;
                }
                i++;
            }
            return false;
        }

        public bool IsNotUsed(int x, int y)
        {
            Rectangle rect = new Rectangle(); 
            foreach (int n in _listSplitRectNotUsed)
            {
                if (GetRectIndex(n, ref rect) && rect.Contains(x, y))
                {                 
                    return true;
                }
            }
            return false;
        }

        public bool IsRectUsed(Rectangle rect)
        {
            Rectangle rectNot = new Rectangle();
            foreach (int n in _listSplitRectNotUsed)
            {
                if (GetRectIndex(n, ref rectNot) && rectNot == rect)
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsRectUsed(int index)
        {
            foreach (int n in _listSplitRectNotUsed)
            {
                if (n == index)
                    return false;
            }
            return true;
        }

        //区块加入切割
        public bool AddRectUsed(int x,int y)
        {
            int i = 0;
            Rectangle rectNot = new Rectangle();
            foreach (int n in _listSplitRectNotUsed)
            {
                if (GetRectIndex(n, ref rectNot) && rectNot.Contains(x,y))
                {
                    _listSplitRectNotUsed.RemoveAt(i);
                    return true;
                }
                i++;
            }
            return false;
        }

        //区块不加入切割
        public bool DelRectUsed(int x, int y)
        {
            int i = 0;
            foreach (Rectangle rect in _listSplitRect)
            {
                if (rect.Contains(x, y))
                {
                    _listSplitRectNotUsed.Add(i);
                    return true; 
                }
                i++;
            }
            return false;
        }

        public bool AddHorLine(int x, int y)
        {
            List<Rectangle> listSplitRectNew = new List<Rectangle>();
            foreach (Rectangle rect in _listSplitRect)
            {
                if (y > rect.Y && y < rect.Y + rect.Height)
                {
                    Rectangle r1 = new Rectangle(rect.Location, rect.Size);
                    r1.Height = y - rect.Y;
                    listSplitRectNew.Add(r1);

                    r1.Y = y ;
                    r1.Height = (rect.Y + rect.Height - y);
                    listSplitRectNew.Add(r1);
                }
                else
                {
                    listSplitRectNew.Add(rect);
                }
            }
            _listSplitRect = listSplitRectNew;
            return true;
        }

        //删除水平线
        public bool DeleteHorSplite(SpliteMoveIndex index)
        {
            List<Rectangle> listSplitRectNew = new List<Rectangle>();
            int i = 0;
            bool del = false;
            Rectangle lastRect = new Rectangle();
            bool haveLast = false;

            foreach (Rectangle rect in _listSplitRect)
            {
                i++;
                if(haveLast)
                {
                    haveLast = false;
                    lastRect.Height += rect.Height;
                    listSplitRectNew.Add(lastRect);
                    continue;
                }

                if(index.rectIndex == i)
                {
                    del = true;
                    if (index.lineIndex == 1)
                    {
                        if(listSplitRectNew.Count == 0)
                        {
                            continue;
                        }
                        else
                        {
                            Rectangle r = listSplitRectNew.Last();
                            r.Height += rect.Height;
                            listSplitRectNew.RemoveAt(listSplitRectNew.Count-1);
                            listSplitRectNew.Add(r);
                        }
                    }
                    else if (index.lineIndex == 2)
                    {
                        if(i == _listSplitRect.Count)
                        {
                            continue;
                        }
                        else
                        {
                            lastRect = rect;
                            haveLast = true;
                        }
                    }
                    else { Debug.Assert(false); }
                }
                else
                {
                    listSplitRectNew.Add(rect);
                }
            }

            _listSplitRect = listSplitRectNew;
            return del;
        }

        public static int GetSplitSize(int widthSrc, int heightSrc, int startX, int startY,
            int widthDest, int heightDest, ref List<Rectangle> listOut)
        {
            listOut = new List<Rectangle>();

            int width = Math.Min(widthSrc - startX, widthDest);

            int i = 0;
            bool stop = false;
            while (!stop)
            {
                Rectangle rect = new Rectangle();

                rect.X = startX;
                rect.Y = startY + (i * heightDest);
                rect.Width = width;
                rect.Height = heightDest;
                if (rect.Y + rect.Height >= heightSrc)
                {
                    stop = true;
                    rect.Height = heightSrc - rect.Y;
                }
                listOut.Add(rect);
                i++;
            }
            return 0;
        }
    }
}
