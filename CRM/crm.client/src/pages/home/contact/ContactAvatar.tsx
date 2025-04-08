import { CommonPage, UserAvatarEditor } from "@etsoo/materialui";
import React from "react";
import { app } from "../../../app/MyApp";
import { useLocation, useNavigate } from "react-router-dom";
import { useParamsEx } from "@etsoo/react";
import { usePageData } from "@etsoo/smarterp-core";
import Stack from "@mui/material/Stack";

export default function ContactAvatar() {
  // Route
  const navigate = useNavigate();
  const { id = 0 } = useParamsEx({ id: "number" });

  const location = useLocation();
  const avatar: string | undefined = location.state;

  // Labels
  const labels = app.getLabels(
    "avatar",
    "editAvatar",
    "imageSizeTooSmall",
    "logo"
  );

  // Page data hook
  usePageData(app, labels.editAvatar, []);

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
              width: "308px",
              height: "300px",
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

            var result = await app.core.memberApi.updateAvatar(id, form);
            if (result == null) return;

            // To view page
            navigate(`./../../view/${id}`);

            // Reset the UI
            return true;
          }}
          maxWidth={640}
        />
      </Stack>
    </CommonPage>
  );
}
