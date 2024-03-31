#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import <Photos/Photos.h>

extern "C" void ShareFile(const char* filePath)
{
    @autoreleasepool {
        NSString *nsFilePath = [NSString stringWithUTF8String:filePath];
        UIImage *image = [UIImage imageWithContentsOfFile:nsFilePath];
        NSString *fileNameWithoutExtension = [[nsFilePath lastPathComponent] stringByDeletingPathExtension];

        // Check permission status
        if (@available(iOS 14, *)) {
            [PHPhotoLibrary requestAuthorizationForAccessLevel: PHAccessLevelAddOnly
                                                       handler: ^(PHAuthorizationStatus status) {
                if (status == PHAuthorizationStatusAuthorized) {
                    UIImageWriteToSavedPhotosAlbum(image, nil, nil, nil);

                    dispatch_async(dispatch_get_main_queue(), ^{
                        UIAlertController *alert = [UIAlertController alertControllerWithTitle:@"Image Saved"
                                                                                       message:[NSString stringWithFormat:@"%@ has been saved to your photo library.", fileNameWithoutExtension]
                                                                                preferredStyle:UIAlertControllerStyleAlert];
                        UIAlertAction *okAction = [UIAlertAction actionWithTitle:@"OK"
                                                                           style:UIAlertActionStyleDefault
                                                                         handler:nil];
                        [alert addAction:okAction];
                        
                        // present the alert
                        UIViewController *rootViewController = [[[UIApplication sharedApplication] windows].firstObject rootViewController];
                        [rootViewController presentViewController:alert animated:YES completion:nil];
                    });
                } else {
                    // Handle not being authorized to access the photo library
                }
            }];
        } else {
            // Fallback on earlier versions
        }
    }
}
