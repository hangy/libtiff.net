﻿/* Copyright (C) 2008-2009, Bit Miracle
 * http://www.bitmiracle.com
 * 
 * This software is based in part on the work of the Sam Leffler, Silicon 
 * Graphics, Inc. and contributors.
 *
 * Copyright (c) 1988-1997 Sam Leffler
 * Copyright (c) 1991-1997 Silicon Graphics, Inc.
 * For conditions of distribution and use, see the accompanying README file.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace BitMiracle.LibTiff.Internal
{
    class CCITTCodecTagMethods : TiffTagMethods
    {
        public override bool vsetfield(Tiff tif, TIFFTAG tag, FieldValue[] ap)
        {
            CCITTCodec sp = (CCITTCodec)tif.m_currentCodec;
            Debug.Assert(sp != null);

            switch (tag)
            {
                case TIFFTAG.TIFFTAG_FAXMODE:
                    sp.m_mode = (FAXMODE)ap[0].ToShort();
                    return true; /* NB: pseudo tag */
                case TIFFTAG.TIFFTAG_FAXFILLFUNC:
                    sp.fill = ap[0].Value as CCITTCodec.FaxFillFunc;
                    return true; /* NB: pseudo tag */
                case TIFFTAG.TIFFTAG_GROUP3OPTIONS:
                    /* XXX: avoid reading options if compression mismatches. */
                    if (tif.m_dir.td_compression == COMPRESSION.COMPRESSION_CCITTFAX3)
                        sp.m_groupoptions = (GROUP3OPT)ap[0].ToShort();
                    break;
                case TIFFTAG.TIFFTAG_GROUP4OPTIONS:
                    /* XXX: avoid reading options if compression mismatches. */
                    if (tif.m_dir.td_compression == COMPRESSION.COMPRESSION_CCITTFAX4)
                        sp.m_groupoptions = (GROUP3OPT)ap[0].ToShort();
                    break;
                case TIFFTAG.TIFFTAG_BADFAXLINES:
                    sp.m_badfaxlines = ap[0].ToUInt();
                    break;
                case TIFFTAG.TIFFTAG_CLEANFAXDATA:
                    sp.m_cleanfaxdata = (CLEANFAXDATA)ap[0].ToByte();
                    break;
                case TIFFTAG.TIFFTAG_CONSECUTIVEBADFAXLINES:
                    sp.m_badfaxrun = ap[0].ToUInt();
                    break;
                case TIFFTAG.TIFFTAG_FAXRECVPARAMS:
                    sp.m_recvparams = ap[0].ToUInt();
                    break;
                case TIFFTAG.TIFFTAG_FAXSUBADDRESS:
                    Tiff.setString(out sp.m_subaddress, ap[0].ToString());
                    break;
                case TIFFTAG.TIFFTAG_FAXRECVTIME:
                    sp.m_recvtime = ap[0].ToUInt();
                    break;
                case TIFFTAG.TIFFTAG_FAXDCS:
                    Tiff.setString(out sp.m_faxdcs, ap[0].ToString());
                    break;
                default:
                    return base.vsetfield(tif, tag, ap);
            }

            TiffFieldInfo fip = tif.FieldWithTag(tag);
            if (fip != null)
                tif.setFieldBit(fip.field_bit);
            else
                return false;

            tif.m_flags |= Tiff.TIFF_DIRTYDIRECT;
            return true;
        }

        public override FieldValue[] vgetfield(Tiff tif, TIFFTAG tag)
        {
            CCITTCodec sp = (CCITTCodec)tif.m_currentCodec;
            Debug.Assert(sp != null);

            FieldValue[] result = new FieldValue[1];

            switch (tag)
            {
                case TIFFTAG.TIFFTAG_FAXMODE:
                    result[0].Set(sp.m_mode);
                    break;
                case TIFFTAG.TIFFTAG_FAXFILLFUNC:
                    result[0].Set(sp.fill);
                    break;
                case TIFFTAG.TIFFTAG_GROUP3OPTIONS:
                case TIFFTAG.TIFFTAG_GROUP4OPTIONS:
                    result[0].Set(sp.m_groupoptions);
                    break;
                case TIFFTAG.TIFFTAG_BADFAXLINES:
                    result[0].Set(sp.m_badfaxlines);
                    break;
                case TIFFTAG.TIFFTAG_CLEANFAXDATA:
                    result[0].Set(sp.m_cleanfaxdata);
                    break;
                case TIFFTAG.TIFFTAG_CONSECUTIVEBADFAXLINES:
                    result[0].Set(sp.m_badfaxrun);
                    break;
                case TIFFTAG.TIFFTAG_FAXRECVPARAMS:
                    result[0].Set(sp.m_recvparams);
                    break;
                case TIFFTAG.TIFFTAG_FAXSUBADDRESS:
                    result[0].Set(sp.m_subaddress);
                    break;
                case TIFFTAG.TIFFTAG_FAXRECVTIME:
                    result[0].Set(sp.m_recvtime);
                    break;
                case TIFFTAG.TIFFTAG_FAXDCS:
                    result[0].Set(sp.m_faxdcs);
                    break;
                default:
                    return base.vgetfield(tif, tag);
            }

            return result;
        }

        public override void printdir(Tiff tif, Stream fd, TiffPrintDirectoryFlags flags)
        {
            CCITTCodec sp = (CCITTCodec)tif.m_currentCodec;
            Debug.Assert(sp != null);

            if (tif.fieldSet(CCITTCodec.FIELD_OPTIONS))
            {
                string sep = " ";
                if (tif.m_dir.td_compression == COMPRESSION.COMPRESSION_CCITTFAX4)
                {
                    Tiff.fprintf(fd, "  Group 4 Options:");
                    if ((sp.m_groupoptions & GROUP3OPT.GROUP3OPT_UNCOMPRESSED) != 0)
                        Tiff.fprintf(fd, "{0}uncompressed data", sep);
                }
                else
                {
                    Tiff.fprintf(fd, "  Group 3 Options:");
                    if ((sp.m_groupoptions & GROUP3OPT.GROUP3OPT_2DENCODING) != 0)
                    {
                        Tiff.fprintf(fd, "{0}2-d encoding", sep);
                        sep = "+";
                    }

                    if ((sp.m_groupoptions & GROUP3OPT.GROUP3OPT_FILLBITS) != 0)
                    {
                        Tiff.fprintf(fd, "{0}EOL padding", sep);
                        sep = "+";
                    }

                    if ((sp.m_groupoptions & GROUP3OPT.GROUP3OPT_UNCOMPRESSED) != 0)
                        Tiff.fprintf(fd, "{0}uncompressed data", sep);
                }

                Tiff.fprintf(fd, " ({0} = 0x{1:x})\n", sp.m_groupoptions, sp.m_groupoptions);
            }

            if (tif.fieldSet(CCITTCodec.FIELD_CLEANFAXDATA))
            {
                Tiff.fprintf(fd, "  Fax Data:");
                
                switch (sp.m_cleanfaxdata)
                {
                    case CLEANFAXDATA.CLEANFAXDATA_CLEAN:
                        Tiff.fprintf(fd, " clean");
                        break;
                    case CLEANFAXDATA.CLEANFAXDATA_REGENERATED:
                        Tiff.fprintf(fd, " receiver regenerated");
                        break;
                    case CLEANFAXDATA.CLEANFAXDATA_UNCLEAN:
                        Tiff.fprintf(fd, " uncorrected errors");
                        break;
                }

                Tiff.fprintf(fd, " ({0} = 0x{1:x})\n", sp.m_cleanfaxdata, sp.m_cleanfaxdata);
            }

            if (tif.fieldSet(CCITTCodec.FIELD_BADFAXLINES))
                Tiff.fprintf(fd, "  Bad Fax Lines: {0}\n", sp.m_badfaxlines);
            
            if (tif.fieldSet(CCITTCodec.FIELD_BADFAXRUN))
                Tiff.fprintf(fd, "  Consecutive Bad Fax Lines: {0}\n", sp.m_badfaxrun);
            
            if (tif.fieldSet(CCITTCodec.FIELD_RECVPARAMS))
                Tiff.fprintf(fd, "  Fax Receive Parameters: {0,8:x}\n", sp.m_recvparams);
            
            if (tif.fieldSet(CCITTCodec.FIELD_SUBADDRESS))
                Tiff.fprintf(fd, "  Fax SubAddress: {0}\n", sp.m_subaddress);
            
            if (tif.fieldSet(CCITTCodec.FIELD_RECVTIME))
                Tiff.fprintf(fd, "  Fax Receive Time: {0} secs\n", sp.m_recvtime);
            
            if (tif.fieldSet(CCITTCodec.FIELD_FAXDCS))
                Tiff.fprintf(fd, "  Fax DCS: {0}\n", sp.m_faxdcs);
        }
    }
}