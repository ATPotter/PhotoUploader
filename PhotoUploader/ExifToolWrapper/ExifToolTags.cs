/* Copyright 2008 Robert C. Brinson <rbrinson@gmail.com>
 * 
 * This file is part of PictureTagger.
 *
 * PictureTagger is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 2 of the License, or
 * (at your option) any later version.
 *
 * PictureTagger is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with PictureTagger.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;

namespace PictureTagger.PictureTaggerUtility
{
    public static class ExifToolTags
    {
        public const string IPTC_HEADLINE = "Headline";
        public const string XMP_HEADLINE = "XMP:Headline";
        public const string IPTC_CAPTION = "Caption-Abstract";
        public const string XMP_DESCRIPTION = "XMP-dc:Description";
        public const string IPTC_KEYWORDS = "Keywords";
        public const string XMP_SUBJECT = "XMP:Subject";
        public const string THUMBNAIL = "ThumbnailImage";
    }
}