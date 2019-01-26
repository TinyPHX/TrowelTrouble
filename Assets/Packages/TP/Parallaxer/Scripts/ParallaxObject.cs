using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TP.ExtensionMethods;
using UnityEngine;
using UnityEngine.UI;

namespace TP.Parallaxer
{
    //[ExecuteInEditMode]
    public class ParallaxObject : MonoBehaviour
    {
        public bool repeat = false;
        public Vector3 repeatOffset;
        public Vector3 positionOffset;
        public bool randomizePosition = false;
        public bool scaleToFill = false;
        public bool scaleSnap = false;
        public bool repeatToFill = false;

        public Image mainImage;

        [SerializeField] private List<ImageWithOffset> imageGroup = new List<ImageWithOffset> { };
        private ParallaxLayer parallaxLayer;
        private Vector2 wrapPoint;

        [System.Serializable]
        public struct ImageWithOffset
        {
            public Image image; 
            public Vector3 offset;

            public ImageWithOffset(Image image, Vector3 offset)
            {
                this.image = image;
                this.offset = offset;
            }
        }

        public void Initialize(ParallaxLayer parentParallaxer)
        {
            parallaxLayer = parentParallaxer;
            
            foreach (ImageWithOffset imageWithOffset in imageGroup)
            {
                if (imageWithOffset.image != mainImage)
                {
                    imageWithOffset.image.gameObject.BlowUp();
                }
            }
            imageGroup.Clear();

            if (scaleToFill)
            {
                mainImage.SetNativeSize();
                Vector2 sizeToScale = Size;
                    
                float scaleX = parallaxLayer.ViewSize.x / sizeToScale.x;
                float scaleY = parallaxLayer.ViewSize.y / sizeToScale.y;
                float scaleFill = Mathf.Max(scaleX, scaleY);

                if (scaleSnap)
                {
                    scaleFill = Mathf.Ceil(scaleFill);
                }

                Size = sizeToScale * scaleFill;
            }
            
            wrapPoint = parallaxLayer.MaxRotationalPosition;
            if (repeat)
            {
                wrapPoint = parallaxLayer.MaxScreenPosition;
            }

            if (Size.x > 0 && Size.y > 0)
            {
                wrapPoint.x = wrapPoint.x - wrapPoint.x % (Size.x);
                wrapPoint.y = wrapPoint.y - wrapPoint.y % (Size.y);
            }
            wrapPoint += Size;

            if (Size.x < parallaxLayer.MaxScreenPosition.x * 2)
            {
                wrapPoint.x += Size.x / 2;
            }

            if (Size.y < parallaxLayer.MaxScreenPosition.y * 2)
            {
                wrapPoint.y += Size.y / 2;
            }

            if (randomizePosition)
            {
                positionOffset = new Vector3(
                    Random.Range(-wrapPoint.x, wrapPoint.x),
                    Random.Range(-wrapPoint.y, wrapPoint.y),
                    positionOffset.z
                    );
            }

            if (repeat && repeatToFill)
            {
                imageGroup = new List<ImageWithOffset> { };

                int xCount = Mathf.CeilToInt(parallaxLayer.ViewSize.x / Size.x) + 1;
                int yCount = Mathf.CeilToInt(parallaxLayer.ViewSize.y / Size.y) + 1;

                for (int ix = 0; ix < xCount; ix++)
                {
                    for (int iy = 0; iy < yCount; iy++)
                    {
                        Image image;
                        if (ix == 0 && iy == 0)
                        {
                            image = mainImage;
                        }
                        else
                        {
                            image = Instantiate(mainImage, transform);
                        }

                        image.transform.localPosition = new Vector3(
                            ix * Size.x,
                            iy * Size.y,
                            image.transform.localPosition.z);

                        imageGroup.Add(new ImageWithOffset(image, image.transform.localPosition));
                    }
                }
            }
        }

        private void UpdatePosition()
        {
            Vector3 wrappedPosition = Position;
            wrappedPosition.x = wrappedPosition.x.WrapBetween(-wrapPoint.x, wrapPoint.x);
            wrappedPosition.y = wrappedPosition.y.WrapBetween(-wrapPoint.y, wrapPoint.y);
            transform.localPosition = wrappedPosition;

            foreach (ImageWithOffset imageWithOffset in imageGroup)
            {
                Vector3 imagePosition = Position + imageWithOffset.offset;
                Vector3 wrappedImagePosition = new Vector3(
                    imagePosition.x.WrapBetween(-wrapPoint.x, wrapPoint.x),
                    imagePosition.y.WrapBetween(-wrapPoint.y, wrapPoint.y),
                    imagePosition.z
                    );

                imageWithOffset.image.transform.localPosition = wrappedImagePosition - Position;
            }
        }

        public Vector2 Size
        {
            get { return mainImage.rectTransform.sizeDelta; }
            set { mainImage.rectTransform.sizeDelta = value; }
        }

        public Vector3 Position
        {
            get { return transform.localPosition; }
            set {
                transform.localPosition = value;
                UpdatePosition();
            }
        }
    }
}
