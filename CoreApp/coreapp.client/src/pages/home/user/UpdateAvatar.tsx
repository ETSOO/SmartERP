import { CommonPage, UserAvatarEditor } from "@etsoo/materialui";
import React from "react";
import { app } from "../../../app/MyApp";
import { CoreUtils, usePageDataEmpty } from "@etsoo/smarterp-core";
import Stack from "@mui/material/Stack";

export default function UpdateAvatar() {
  // Labels
  const labels = app.getLabels("avatar", "imageSizeTooSmall");

  // User context
  const Context = app.userState.context;

  // Page data hook
  usePageDataEmpty(app);

  return (
    <CommonPage sx={{ width: "fit-content" }}>
      <Stack direction={{ xs: "column", sm: "column", md: "row" }} spacing={1}>
        <Context.Consumer>
          {(user) => {
            const avatar = user.state.avatar;
            return avatar == null ? (
              <React.Fragment />
            ) : (
              <img
                src={avatar}
                alt={labels.avatar}
                style={CoreUtils.avatarStyles(false)}
              />
            );
          }}
        </Context.Consumer>
        <UserAvatarEditor
          onDone={async (canvas, toBlob, type) => {
            // Check size
            if (canvas.width < 100 || canvas.height < 100) {
              app.notifier.alert(labels.imageSizeTooSmall);
              return;
            }

            // Photo blob
            const blob = await toBlob(canvas, type, 1);

            // Form data
            const form = new FormData();
            form.append("avatar", blob);

            var result = await app.core.userApi.updateAvatar(form);
            if (result == null) return;

            // Refresh token to get the updated avatar
            app.refreshToken({ showLoading: true });

            // Reset the UI
            return true;
          }}
          maxWidth={640}
        />
      </Stack>
    </CommonPage>
  );
}
