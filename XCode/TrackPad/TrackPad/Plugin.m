#import <Foundation/Foundation.h>
@import AppKit;

#define RING_BUFFER_SIZE 512

#define PHASE_BEGAN 0
#define PHASE_MOVED 1
#define PHASE_ENDED 2
#define PHASE_CANCELLED 3
#define PHASE_STATIONARY 4

struct NativeTouchEvent {
  unsigned char touchId;
  unsigned char phase;
  float normalX;
  float normalY;
  float deviceWidth;
  float deviceHeight;
};

#define ringBuffer_typedef(T, NAME) \
  typedef struct { \
    int size; \
    int start; \
    int end; \
    T* elems; \
  } NAME

#define bufferInit(BUF, S, T) \
  BUF.size = S+1; \
  BUF.start = 0; \
  BUF.end = 0; \
  BUF.elems = (T*)calloc(BUF.size, sizeof(T))


#define bufferDestroy(BUF) if (BUF.elems) { free(BUF.elems); BUF.elems = 0; }
#define nextStartIndex(BUF) ((BUF.start + 1) % BUF.size)
#define nextEndIndex(BUF) ((BUF.end + 1) % BUF.size)
#define isBufferEmpty(BUF) (BUF.end == BUF.start)
#define isBufferFull(BUF) (nextEndIndex(BUF) == BUF.start)

#define bufferWrite(BUF, ELEM) \
  BUF.elems[BUF.end] = ELEM; \
  BUF.end = (BUF.end + 1) % BUF.size; \
  if (isBufferEmpty(BUF)) { \
    BUF.start = nextStartIndex(BUF); \
  }

#define bufferRead(BUF, ELEM) \
    ELEM = BUF.elems[BUF.start]; \
    BUF.start = nextStartIndex(BUF);

ringBuffer_typedef(struct NativeTouchEvent, TouchEventRing);

TouchEventRing ringBuffer = {0, 0, 0, NULL};

NSView* view = nil;

@interface TrackingObject : NSResponder
{
}
- (void)touchesBeganWithEvent:(NSEvent *)event;
- (void)touchesEndedWithEvent:(NSEvent *)event;
- (void)touchesMovedWithEvent:(NSEvent *)event;
- (void)touchesCancelledWithEvent:(NSEvent *)event;
- (void)magnifyWithEvent:(NSEvent *)event;
- (void)swipeWithEvent:(NSEvent *)event;
@end

@implementation TrackingObject

struct Finger {
    void* nativeTouchId;
    double time;
};

#define MAX_FINGERS 10
#define FINGER_TIMEOUT_SECS 1.5

struct Finger fingers[MAX_FINGERS];


void DebugDump() {
    NSLog(@"----\n");
    for (int i = 0; i < MAX_FINGERS; ++i) {
        NSLog(@"finger %d: (nativeTouchId=%p, time=%f)\n", i, fingers[i].nativeTouchId, fingers[i].time);
    }
    
}

static void clearFingers() {
    for (int i = 0; i < MAX_FINGERS; ++i) {
        if (fingers[i].nativeTouchId != NULL) {
            fingers[i].nativeTouchId = NULL;
            fingers[i].time = 0;
            struct NativeTouchEvent event = {i, PHASE_ENDED, 0, 0};
            bufferWrite(ringBuffer, event);
        }
    }
}

static void clearFingersOlderThan(double time) {
    for (int i = 0; i < MAX_FINGERS; ++i) {
        if (fingers[i].time < time && fingers[i].nativeTouchId != NULL) {
            fingers[i].nativeTouchId = NULL;
            fingers[i].time = time;
            struct NativeTouchEvent event = {i, PHASE_ENDED, 0, 0};
            bufferWrite(ringBuffer, event);
        }
    }
}

static unsigned char getUserTouchId(void* touchId, double time) {
    for (int i = 0; i < MAX_FINGERS; ++i) {
        if (fingers[i].nativeTouchId == touchId) {
            fingers[i].time = time;
            return i;
        }
    }
    
    for (int i = 0; i < MAX_FINGERS; ++i) {
        if (fingers[i].nativeTouchId == NULL) {
            fingers[i].nativeTouchId = touchId;
            fingers[i].time = time;
            return i;
        }
    }
    
    double minTime = DBL_MAX;
    int minIndex = 0;
    for (int i = 0; i < MAX_FINGERS; ++i) {
        if (fingers[i].time < minTime) {
            minTime = fingers[i].time;
            minIndex = i;
        }
    }
    
    fingers[minIndex].time = time;
    
    return minIndex;
}

static void releaseTouchId(int userTouchId) {
    assert(userTouchId < MAX_FINGERS);
    fingers[userTouchId].nativeTouchId = NULL;
}

static void HandleTouchEvent(NSEvent* event, const char* debugStr) {
    double time = event.timestamp;
    
    NSSet *touches = [event touchesMatchingPhase:NSTouchPhaseAny inView:view];
    for (NSTouch *touch in touches)
    {
        void* touchId = touch.identity;
        unsigned char userTouchId = getUserTouchId(touchId, time);

        switch ([touch phase]) {
            case NSTouchPhaseBegan: {
                //NSLog(@"TPT BEGAN: %p %s\n", touch.identity, debugStr);
                float normalX = touch.normalizedPosition.x;
                float normalY = touch.normalizedPosition.y;
                struct NativeTouchEvent event = {userTouchId, PHASE_BEGAN, normalX, normalY, touch.deviceSize.width, touch.deviceSize.height};
                bufferWrite(ringBuffer, event);
                break;
            }
            case NSTouchPhaseMoved: {
                //NSLog(@"TPT MOVED: %p %s\n", touch.identity, debugStr);
                float normalX = touch.normalizedPosition.x;
                float normalY = touch.normalizedPosition.y;
                struct NativeTouchEvent event = {userTouchId, PHASE_MOVED, normalX, normalY, touch.deviceSize.width, touch.deviceSize.height};
                bufferWrite(ringBuffer, event);
                break;
            }
            case NSTouchPhaseCancelled: {
//                NSLog(@"TPT CANCELLED: %p %s\n", touch.identity, debugStr);
                struct NativeTouchEvent event = {userTouchId, PHASE_CANCELLED, 0, 0, touch.deviceSize.width, touch.deviceSize.height};
                releaseTouchId(userTouchId);
                bufferWrite(ringBuffer, event);
                break;
            }
            case NSTouchPhaseStationary: {
//                NSLog(@"TPT STATIONARY: %p %s\n", touch.identity, debugStr);
                float normalX = touch.normalizedPosition.x;
                float normalY = touch.normalizedPosition.y;
                struct NativeTouchEvent event = {userTouchId, PHASE_STATIONARY, normalX, normalY, touch.deviceSize.width, touch.deviceSize.height};
                bufferWrite(ringBuffer, event);
                break;
            }
            case NSTouchPhaseEnded: {
//                NSLog(@"ENDED: %p %s\n", touch.identity, debugStr);
                struct NativeTouchEvent event = {userTouchId, PHASE_ENDED, 0, 0, touch.deviceSize.width, touch.deviceSize.height};
                releaseTouchId(userTouchId);
                bufferWrite(ringBuffer, event);
                break;
            }
            default:
//                NSLog(@"Unknown event type\n");
                break;
        }
    }
    
    clearFingersOlderThan(time - FINGER_TIMEOUT_SECS);

}



- (void)touchesBeganWithEvent:(NSEvent *)event
{
    HandleTouchEvent(event, "began");
}

- (void)touchesMovedWithEvent:(NSEvent *)event
{
    HandleTouchEvent(event, "moved");
}

- (void)touchesEndedWithEvent:(NSEvent *)event
{
    HandleTouchEvent(event, "ended");
}

- (void)touchesCancelledWithEvent:(NSEvent *)event
{
    HandleTouchEvent(event, "cancelled");
}

- (void)magnifyWithEvent:(NSEvent *)event {
    // unity's game mode will maximmize and minimize if we don't receive this event.
}

- (void)swipeWithEvent:(NSEvent *)event {}

@end

TrackingObject* pTrackMgr = nil;

void DestroyTrackingObject()
{
  if (ringBuffer.elems)
     bufferDestroy(ringBuffer);
    
    if(pTrackMgr != nil)
    {
        [view setNextResponder:[pTrackMgr nextResponder]];
        [pTrackMgr release];
        pTrackMgr = nil;
    }
}

void SetupTrackingObject()
{
    NSApplication* app = [NSApplication sharedApplication];
    NSWindow* window = [app mainWindow];
    view = [window contentView];
    
    DestroyTrackingObject();

    bufferInit(ringBuffer, RING_BUFFER_SIZE, struct NativeTouchEvent);
    
    clearFingers();
    
    pTrackMgr = [TrackingObject alloc];
    NSResponder* responder = [view nextResponder];
    [view setNextResponder:pTrackMgr];
    [pTrackMgr setNextResponder:responder];

    [view setAcceptsTouchEvents:YES];
    [view setWantsRestingTouches: TRUE];
}

void InitPlugin()
{
//    NSLog(@"TPT InitPlugin");
    SetupTrackingObject();
}

void ClearTouches() {
    if (ringBuffer.elems)
        bufferDestroy(ringBuffer);
    bufferInit(ringBuffer, RING_BUFFER_SIZE, struct NativeTouchEvent);
    clearFingers();
}

bool ReadTouchEvent(struct NativeTouchEvent* event) {
    if (!isBufferEmpty(ringBuffer)) {
        bufferRead(ringBuffer, (*event));
        return true;
    } else {
        return false;
    }
}

void DeinitPlugin() {
    DestroyTrackingObject();
}


