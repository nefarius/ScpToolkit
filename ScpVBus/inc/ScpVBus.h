//{F679F562-3164-42CE-A4DB-E7DDBE723909}  
DEFINE_GUID (GUID_DEVINTERFACE_SCPVBUS, 0xf679f562, 0x3164, 0x42ce, 0xa4, 0xdb, 0xe7, 0xdd, 0xbe, 0x72, 0x39, 0x9);

#ifndef __SCPVBUS_H
#define __SCPVBUS_H

#define BUSENUM_IOCTL(_index_)	CTL_CODE(FILE_DEVICE_BUSENUM, _index_, METHOD_BUFFERED, FILE_READ_DATA)

#define IOCTL_BUSENUM_PLUGIN_HARDWARE	BUSENUM_IOCTL(0x0)
#define IOCTL_BUSENUM_UNPLUG_HARDWARE	BUSENUM_IOCTL(0x1)
#define IOCTL_BUSENUM_EJECT_HARDWARE	BUSENUM_IOCTL(0x2)
#define IOCTL_BUSENUM_REPORT_HARDWARE	BUSENUM_IOCTL(0x3)

#define REPORT_SIZE	20

//
//  Data structures used in User IoCtls
//

typedef struct _BUSENUM_PLUGIN_HARDWARE {

    __in ULONG Size;                          
  
    __in ULONG SerialNo;

    ULONG Reserved[2];    
                                                                        
} BUSENUM_PLUGIN_HARDWARE, *PBUSENUM_PLUGIN_HARDWARE;

typedef struct _BUSENUM_UNPLUG_HARDWARE {

    __in ULONG Size;                                    

    __in ULONG SerialNo;
    
    ULONG Reserved[2];    

} BUSENUM_UNPLUG_HARDWARE, *PBUSENUM_UNPLUG_HARDWARE;

typedef struct _BUSENUM_EJECT_HARDWARE {

    __in ULONG Size;                                    

    __in ULONG SerialNo;
    
    ULONG Reserved[2];    

} BUSENUM_EJECT_HARDWARE, *PBUSENUM_EJECT_HARDWARE;

typedef struct _BUSENUM_REPORT_HARDWARE {

    __in ULONG Size;                                    

    __in ULONG SerialNo;
    
    UCHAR Data[REPORT_SIZE]; 

} BUSENUM_REPORT_HARDWARE, *PBUSENUM_REPORT_HARDWARE;

#endif
