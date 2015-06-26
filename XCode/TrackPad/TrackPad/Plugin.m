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

- (void)touchesBeganWithEvent:(NSEvent *)event
{
    NSSet *touches = [event touchesMatchingPhase:NSTouchPhaseBegan inView:view];
    for (NSTouch *touch in touches)
    {
        void* touchId = touch.identity;
        float normalizedX = touch.normalizedPosition.x;
        float normalizedY = touch.normalizedPosition.y;
        void *args[] = { &touchId, &normalizedX, &normalizedY };
        mono_runtime_invoke(trackPadTouchBeganMethod, NULL, args, NULL);
    }
}

- (void)touchesMovedWithEvent:(NSEvent *)event
{
    NSSet *touches = [event touchesMatchingPhase:NSTouchPhaseMoved inView:view];
    for (NSTouch *touch in touches)
    {
        void* touchId = touch.identity;
        float normalizedX = touch.normalizedPosition.x;
        float normalizedY = touch.normalizedPosition.y;
        void *args[] = { &touchId, &normalizedX, &normalizedY };
        mono_runtime_invoke(trackPadTouchMovedMethod, NULL, args, NULL);
    }
}

- (void)touchesEndedWithEvent:(NSEvent *)event
{
    NSSet *touches = [event touchesMatchingPhase:NSTouchPhaseEnded inView:view];
    for (NSTouch *touch in touches)
    {
        void* touchId = touch.identity;
        void *args[] = { &touchId };
        mono_runtime_invoke(trackPadTouchEndedMethod, NULL, args, NULL);
    }
    
}

- (void)touchesCancelledWithEvent:(NSEvent *)event
{
    NSSet *touches = [event touchesMatchingPhase:NSTouchPhaseCancelled inView:view];
    for (NSTouch *touch in touches)
    {
        void* touchId = touch.identity;
        void *args[] = { &touchId };
        mono_runtime_invoke(trackPadTouchCancelledMethod, NULL, args, NULL);
    }
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
        [pTrackMgr release];
        pTrackMgr = nil;
    }
    
    pTrackMgr = [TrackingObject alloc];
    [view setAcceptsTouchEvents:YES];
    [view setNextResponder:pTrackMgr];
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
    
    trackPadTouchBeganDesc = mono_method_desc_new("TrackPadInput:TrackPadTouchBegan", FALSE);
    trackPadTouchBeganMethod = mono_method_desc_search_in_image(trackPadTouchBeganDesc, monoImage);
    mono_method_desc_free(trackPadTouchBeganDesc);

    trackPadTouchMovedDesc = mono_method_desc_new("TrackPadInput:TrackPadTouchMoved", FALSE);
    trackPadTouchMovedMethod = mono_method_desc_search_in_image(trackPadTouchMovedDesc, monoImage);
    mono_method_desc_free(trackPadTouchMovedDesc);
    
    trackPadTouchEndedDesc = mono_method_desc_new("TrackPadInput:TrackPadTouchEnded", FALSE);
    trackPadTouchEndedMethod = mono_method_desc_search_in_image(trackPadTouchEndedDesc, monoImage);
    mono_method_desc_free(trackPadTouchEndedDesc);
    
    trackPadTouchCancelledDesc = mono_method_desc_new("TrackPadInput:TrackPadTouchCancelled", FALSE);
    trackPadTouchCancelledMethod = mono_method_desc_search_in_image(trackPadTouchCancelledDesc, monoImage);
    mono_method_desc_free(trackPadTouchCancelledDesc);
}

void DeinitPlugin() {
}


