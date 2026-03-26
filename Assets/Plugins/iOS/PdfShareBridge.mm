#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

static UIViewController *PdfShareBridgeTopViewController(void)
{
    UIViewController *rootViewController = nil;

    if (@available(iOS 13.0, *))
    {
        NSSet<UIScene *> *connectedScenes = [UIApplication sharedApplication].connectedScenes;

        for (UIScene *scene in connectedScenes)
        {
            if (![scene isKindOfClass:[UIWindowScene class]])
            {
                continue;
            }

            UIWindowScene *windowScene = (UIWindowScene *)scene;

            for (UIWindow *window in windowScene.windows)
            {
                if (window.isKeyWindow)
                {
                    rootViewController = window.rootViewController;
                    break;
                }
            }

            if (rootViewController != nil)
            {
                break;
            }
        }
    }

    if (rootViewController == nil)
    {
        rootViewController = [UIApplication sharedApplication].keyWindow.rootViewController;
    }

    while (rootViewController.presentedViewController != nil)
    {
        rootViewController = rootViewController.presentedViewController;
    }

    return rootViewController;
}

extern "C" void SharePdfAtPath(const char *filePath)
{
    if (filePath == NULL)
    {
        return;
    }

    NSString *path = [NSString stringWithUTF8String:filePath];

    if (path.length == 0 || ![[NSFileManager defaultManager] fileExistsAtPath:path])
    {
        NSLog(@"[PdfShareBridge] PDF file missing at path: %@", path);
        return;
    }

    dispatch_async(dispatch_get_main_queue(), ^{
        UIViewController *topViewController = PdfShareBridgeTopViewController();

        if (topViewController == nil)
        {
            NSLog(@"[PdfShareBridge] No view controller available to present share sheet.");
            return;
        }

        NSURL *fileUrl = [NSURL fileURLWithPath:path];
        UIActivityViewController *activityController =
            [[UIActivityViewController alloc] initWithActivityItems:@[ fileUrl ]
                                              applicationActivities:nil];

        UIPopoverPresentationController *popover = activityController.popoverPresentationController;

        if (popover != nil)
        {
            popover.sourceView = topViewController.view;
            popover.sourceRect =
                CGRectMake(CGRectGetMidX(topViewController.view.bounds),
                           CGRectGetMidY(topViewController.view.bounds),
                           1.0,
                           1.0);
            popover.permittedArrowDirections = 0;
        }

        [topViewController presentViewController:activityController animated:YES completion:nil];
    });
}
