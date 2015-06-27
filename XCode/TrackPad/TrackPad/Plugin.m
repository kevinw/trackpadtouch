#import <Foundation/Foundation.h>
@import AppKit;
#import "TrackPad_Prefix.pch"

MonoMethod *trackPadTouchBeganMethod;
MonoMethod *trackPadTouchMovedMethod;
MonoMethod *trackPadTouchEndedMethod;
MonoMethod *trackPadTouchStationaryMethod;
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


static void FindMethod(MonoImage* monoImage, const char* methodName, MonoMethod** pointer) {
    MonoMethodDesc *desc = mono_method_desc_new(methodName, TRUE);
    if (!desc) {
        NSLog(@"Could not find method %s", methodName);
        return;
    }
    
    *pointer = mono_method_desc_search_in_image(desc, monoImage);
    mono_method_desc_free(desc);
}

void InitPlugin(const char* pluginPath)
{
    trackPadTouchBeganMethod = NULL;
    trackPadTouchMovedMethod = NULL;
    trackPadTouchEndedMethod = NULL;
    trackPadTouchStationaryMethod = NULL;
    trackPadTouchCancelledMethod = NULL;
    
    SetupTrackingObject();
    
    NSString *assemblyPath = [NSString stringWithUTF8String:pluginPath];
    
    MonoDomain* domain = mono_domain_get();
    MonoAssembly* monoAssembly = mono_domain_assembly_open(domain, assemblyPath.UTF8String);
    MonoImage* monoImage = mono_assembly_get_image(monoAssembly);
    
    FindMethod(monoImage, "TrackpadTouch.TrackpadInput:TrackpadTouchBegan", &trackPadTouchBeganMethod);
    FindMethod(monoImage, "TrackpadTouch.TrackpadInput:TrackpadTouchMoved", &trackPadTouchMovedMethod);
    FindMethod(monoImage, "TrackpadTouch.TrackpadInput:TrackpadTouchEnded", &trackPadTouchEndedMethod);
    FindMethod(monoImage, "TrackpadTouch.TrackpadInput:TrackpadTouchStationary", &trackPadTouchStationaryMethod);
    FindMethod(monoImage, "TrackpadTouch.TrackpadInput:TrackpadTouchCancelled", &trackPadTouchCancelledMethod);
}

void DeinitPlugin() {
}


