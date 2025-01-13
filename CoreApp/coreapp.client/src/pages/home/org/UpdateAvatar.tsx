import { CommonPage, UserAvatarEditor } from "@etsoo/materialui";
import { Stack } from "@mui/material";
import React from "react";
import { app } from "../../../app/MyApp";
import { PageDataContext } from "@etsoo/toolpad";
import { useLocation, useNavigate } from "react-router-dom";
import { useParamsEx } from "@etsoo/react";

export default function UpdateAvatar() {
  // Page data
  const { dispatch } = React.useContext(PageDataContext);

  // Route
  const navigate = useNavigate();
  const { id = 0 } = useParamsEx({ id: "number" });

  const location = useLocation();
  const avatar: string | undefined = location.state;

  // Labels
  const labels = app.getLabels(
    "avatar",
    "editLogo",
    "imageSizeTooSmall",
    "logo"
  );

  React.useEffect(() => {
    // Page title
    dispatch({ page: labels.editLogo });

    return () => {
      app.pageExit();
    };
  }, []);

  return (
    <CommonPage sx={{ width: "fit-content" }}>
      <Stack direction={{ xs: "column", sm: "column", md: "row" }} spacing={1}>
        {avatar == null ? (
          <React.Fragment />
        ) : (
          <img
            src={avatar}
            alt={labels.logo}
            style={{
              width: "320px",
              height: "160px",
              border: "1px solid #666"
            }}
          />
        )}
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

            var result = await app.core.orgApi.updateAvatar(id, form);
            if (result == null) return;

            // To view page
            navigate(`./../../my/${id}`);

            // Reset the UI
            return true;
          }}
          width={320}
          height={160}
          maxWidth={640}
        />
      </Stack>
    </CommonPage>
  );
}
