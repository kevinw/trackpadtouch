#import <Foundation/Foundation.h>
@import AppKit;
#import "TrackPad_Prefix.pch"


MonoDomain *domain;
NSString *assemblyPath;
MonoAssembly *monoAssembly;
MonoImage *monoImage;

MonoMethodDesc *trackPadTouchBeganDesc;
MonoMethod *trackPadTouchBeganMethod;

MonoMethodDesc *trackPadTouchMovedDesc;
MonoMethod *trackPadTouchMovedMethod;

MonoMethodDesc *trackPadTouchEndedDesc;
MonoMethod *trackPadTouchEndedMethod;

MonoMethodDesc *trackPadTouchStationaryDesc;
MonoMethod *trackPadTouchStationaryMethod;

MonoMethodDesc *trackPadTouchCancelledDesc;
MonoMethod *trackPadTouchCancelledMethod;

NSView* view = nil;

@interface TrackingObject : NSResponder
{
}
- (void)touchesBeganWithEvent:(NSEvent *)event;
- (void)touchesEndedWithEvent:(NSEvent *)event;
- (void)touchesCancelledWithEvent:(NSEvent *)event;

@end

@implementation TrackingObject

static void HandleTouchEvent(NSEvent* event) {
    NSSet *touches = [event touchesMatchingPhase:NSTouchPhaseAny inView:view];
    for (NSTouch *touch in touches)
    {
        void* touchId = touch.identity;
        switch ([touch phase]) {
            case NSTouchPhaseBegan: {
                float normalX = touch.normalizedPosition.x;
                float normalY = touch.normalizedPosition.y;
                float deviceWidth = touch.deviceSize.width;
                float deviceHeight = touch.deviceSize.height;
                void *args[] = { &touchId, &normalX, &normalY, &deviceWidth, &deviceHeight };
                if (trackPadTouchBeganMethod)
                    mono_runtime_invoke(trackPadTouchBeganMethod, NULL, args, NULL);
                break;
            }
            case NSTouchPhaseMoved: {
                float normalX = touch.normalizedPosition.x;
                float normalY = touch.normalizedPosition.y;
                float deviceWidth = touch.deviceSize.width;
                float deviceHeight = touch.deviceSize.height;
                void *args[] = { &touchId, &normalX, &normalY, &deviceWidth, &deviceHeight };
                if (trackPadTouchMovedMethod)
                    mono_runtime_invoke(trackPadTouchMovedMethod, NULL, args, NULL);
                break;
            }
            case NSTouchPhaseCancelled: {
                void *args[] = { &touchId };
                if (trackPadTouchCancelledMethod)
                    mono_runtime_invoke(trackPadTouchCancelledMethod, NULL, args, NULL);
                break;
            }
            case NSTouchPhaseStationary: {
                float normalX = touch.normalizedPosition.x;
                float normalY = touch.normalizedPosition.y;
                float deviceWidth = touch.deviceSize.width;
                float deviceHeight = touch.deviceSize.height;
                void *args[] = { &touchId, &normalX, &normalY, &deviceWidth, &deviceHeight };
                if (trackPadTouchStationaryMethod)
                    mono_runtime_invoke(trackPadTouchStationaryMethod, NULL, args, NULL);
                break;
            }
            case NSTouchPhaseEnded: {
                void *args[] = { &touchId };
                if (trackPadTouchEndedMethod)
                    mono_runtime_invoke(trackPadTouchEndedMethod, NULL, args, NULL);
                break;
                
            }
            case NSTouchPhaseTouching:
            case NSTouchPhaseAny:
                break;
            default:
                break;
        }
    }
}



- (void)touchesBeganWithEvent:(NSEvent *)event
{
    HandleTouchEvent(event);
}

- (void)touchesMovedWithEvent:(NSEvent *)event
{
    HandleTouchEvent(event);
}

- (void)touchesEndedWithEvent:(NSEvent *)event
{
    HandleTouchEvent(event);
}

- (void)touchesCancelledWithEvent:(NSEvent *)event
{
    HandleTouchEvent(event);
}

- (void)magnifyWithEvent:(NSEvent *)event {
    // block unity from the
}

- (void)swipeWithEvent:(NSEvent *)event {
}


@end

TrackingObject* pTrackMgr = nil;

void SetupTrackingObject()
{
    //    NSLog(@"Native plugin -> SetupTrackingObject");
    
    NSApplication* app = [NSApplication sharedApplication];
    NSWindow* window = [app mainWindow];
    view = [window contentView];
    
    if(pTrackMgr != nil)
    {
        [view setNextResponder:[pTrackMgr nextResponder]];
        [pTrackMgr release];
        pTrackMgr = nil;
    }
    
    pTrackMgr = [TrackingObject alloc];
    [view setAcceptsTouchEvents:YES];
    NSResponder* responder = [view nextResponder];
    [view setNextResponder:pTrackMgr];
    [pTrackMgr setNextResponder:responder];
    [view setWantsRestingTouches: TRUE];
}

void InitPlugin(const char* pluginPath)
{
//    NSLog(@"Native plugin -> PluginInit - device? : %d", isDevice);
    
    SetupTrackingObject();
    
    assemblyPath = [NSString stringWithUTF8String:pluginPath];
    
//    NSLog(@"Native plugin -> assembly path: %@", assemblyPath);
    
    domain = mono_domain_get();
    monoAssembly = mono_domain_assembly_open(domain, assemblyPath.UTF8String);
    monoImage = mono_assembly_get_image(monoAssembly);
    
    trackPadTouchBeganDesc = mono_method_desc_new("TrackpadTouch.TrackpadInput:TrackpadTouchBegan", TRUE);
    trackPadTouchBeganMethod = mono_method_desc_search_in_image(trackPadTouchBeganDesc, monoImage);
    mono_method_desc_free(trackPadTouchBeganDesc);

    trackPadTouchMovedDesc = mono_method_desc_new("TrackpadTouch.TrackpadInput:TrackpadTouchMoved", TRUE);
    trackPadTouchMovedMethod = mono_method_desc_search_in_image(trackPadTouchMovedDesc, monoImage);
    mono_method_desc_free(trackPadTouchMovedDesc);
    
    trackPadTouchEndedDesc = mono_method_desc_new("TrackpadTouch.TrackpadInput:TrackpadTouchEnded", TRUE);
    trackPadTouchEndedMethod = mono_method_desc_search_in_image(trackPadTouchEndedDesc, monoImage);
    mono_method_desc_free(trackPadTouchEndedDesc);
    
    trackPadTouchStationaryDesc = mono_method_desc_new("TrackpadTouch.TrackpadInput:TrackpadTouchStationary", TRUE);
    trackPadTouchStationaryMethod = mono_method_desc_search_in_image(trackPadTouchStationaryDesc, monoImage);
    mono_method_desc_free(trackPadTouchStationaryDesc);

    
    trackPadTouchCancelledDesc = mono_method_desc_new("TrackpadTouch.TrackpadInput:TrackpadTouchCancelled", FALSE);
    trackPadTouchCancelledMethod = mono_method_desc_search_in_image(trackPadTouchCancelledDesc, monoImage);
    mono_method_desc_free(trackPadTouchCancelledDesc);
}

void DeinitPlugin() {
}


