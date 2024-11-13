import {
  CommonPage,
  UserAvatarEditor,
  UserAvatarEditorToBlob
} from "@etsoo/materialui";
import { Stack } from "@mui/material";
import React from "react";
import { useNavigate } from "react-router-dom";
import { app } from "../../../app/MyApp";

export default function UpdateAvatar() {
  // Route
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels("avatar");

  // User context
  const Context = app.userState.context;

  const handleDone = async (
    canvas: HTMLCanvasElement,
    toBlob: UserAvatarEditorToBlob,
    type: string
  ) => {
    // Photo blob
    const blob = await toBlob(canvas, type, 1);

    // Form data
    const form = new FormData();
    form.append("avatar", blob);

    var result = await app.userApi.uploadAvatar(form);
    if (result == null) return;

    // Refresh token to get the updated avatar
    app.refreshToken().then(() => {
      navigate("./../../");
    });
  };

  React.useEffect(() => {
    // Page title
    app.setPageKey("updateAvatar");
  }, []);

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
                style={{
                  width: "308px",
                  height: "300px",
                  border: "1px solid #666"
                }}
              />
            );
          }}
        </Context.Consumer>
        <UserAvatarEditor onDone={handleDone} maxWidth={600} />
      </Stack>
    </CommonPage>
  );
}
